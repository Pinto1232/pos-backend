using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using PosBackend.Application.Interfaces;
using PosBackend.Application.Services;
using PosBackend.Application.Services.Caching;
using PosBackend.Filters;
using PosBackend.Middlewares;
using PosBackend.Models;
using PosBackend.Data;
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Events;
using PosBackend.Security;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "POS Backend")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        retainedFileCountLimit: 7,
        fileSizeLimitBytes: 10 * 1024 * 1024) 
    .CreateLogger();

// Use Serilog for logging
builder.Host.UseSerilog();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",  
                "http://localhost:8080",   
                "http://localhost:8282"    
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetIsOriginAllowed(origin => {
                // Allow localhost with any port for development
                if (origin.StartsWith("http://localhost:")) return true;
                return false;
            });
    });
});

// Add JWT Bearer authentication for Keycloak and API Key authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
        // Disable HTTPS metadata requirement in development
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        // Add handler to trust development certificates
        options.BackchannelHttpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => 
                builder.Environment.IsDevelopment()
        };
        // Configure token validation for development
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // Add clock skew tolerance
            ClockSkew = TimeSpan.FromMinutes(5)
        };
        
        // Add events for better logging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("JWT authentication failed: {Exception}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogDebug("JWT token validated for user: {UserId}", 
                    context.Principal?.Identity?.Name ?? "Unknown");
                return Task.CompletedTask;
            }
        };
    })
    .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationSchemeOptions.Scheme, options => { });

// Add Authorization policies
builder.Services.AddAuthorization(options =>
{
    // Default policy requires authentication
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, ApiKeyAuthenticationSchemeOptions.Scheme)
        .Build();

    // Specific policies
    options.AddPolicy(SecurityConstants.Policies.RequireAuthentication, policy =>
        policy.RequireAuthenticatedUser()
              .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));

    options.AddPolicy(SecurityConstants.Policies.RequireAdmin, policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole(SecurityConstants.Roles.Admin, SecurityConstants.Roles.SystemAdmin));

    options.AddPolicy(SecurityConstants.Policies.RequireUser, policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole(SecurityConstants.Roles.User, SecurityConstants.Roles.Admin, SecurityConstants.Roles.SystemAdmin));

    options.AddPolicy(SecurityConstants.Policies.RequireValidSubscription, policy =>
        policy.RequireAuthenticatedUser()
              .AddRequirements(new ValidSubscriptionRequirement()));

    options.AddPolicy(SecurityConstants.Policies.RequirePackageManagement, policy =>
        policy.RequireAuthenticatedUser()
              .AddRequirements(new PackageManagementRequirement()));

    options.AddPolicy(SecurityConstants.Policies.RequireSystemAdmin, policy =>
        policy.RequireAuthenticatedUser()
              .AddRequirements(new SystemAdminRequirement())
              .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, ApiKeyAuthenticationSchemeOptions.Scheme));

    // Anonymous policies for specific read-only operations
    options.AddPolicy(SecurityConstants.Policies.AllowAnonymousRead, policy =>
        policy.RequireAssertion(context => true)); // Always allow

    options.AddPolicy(SecurityConstants.Policies.AllowAnonymousHealthCheck, policy =>
        policy.RequireAssertion(context => true)); // Always allow
});

// Register authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, ValidSubscriptionHandler>();
builder.Services.AddScoped<IAuthorizationHandler, PackageManagementHandler>();
builder.Services.AddScoped<IAuthorizationHandler, SystemAdminHandler>();

// Load environment variables
DotEnv.Load();

// Register PosDbContext for EF Core and Identity
builder.Services.AddDbContext<PosDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (connectionString == "InMemory")
    {
        options.UseInMemoryDatabase("PosDatabase");
    }
    else
    {
        // Use Npgsql for PostgreSQL, or change to UseSqlServer/UseSqlite as needed
        options.UseNpgsql(connectionString);
    }
});

// Add services to the container
// Add memory cache for both distributed and local caching
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();

// Configure session
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".POS.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.IdleTimeout = TimeSpan.FromMinutes(
        int.Parse(Environment.GetEnvironmentVariable("SESSION_TIMEOUT_MINUTES") ?? "60")
    );
});

// Caching services
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<CacheConfiguration>();
builder.Services.AddScoped<ICacheService, MemoryCacheService>();

// Identity, UserRepo and other services
builder.Services.AddIdentity<User, UserRole>()
    .AddEntityFrameworkStores<PosDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserRepository, UserService>();
builder.Services.AddScoped<PosBackend.Services.PackageFeatureService>();
builder.Services.AddScoped<PosBackend.Services.KeycloakAuthorizationService>();
builder.Services.AddScoped<PosBackend.Services.SubscriptionService>();

// Register Input Sanitization Services
builder.Services.AddScoped<PosBackend.Services.IInputSanitizationService, PosBackend.Services.InputSanitizationService>();
builder.Services.AddScoped<PosBackend.Filters.InputSanitizationFilter>();

// Configure pricing configuration
builder.Services.Configure<PosBackend.Services.PricingOptions>(
    builder.Configuration.GetSection("Pricing"));

// Register GeoLocationService
builder.Services.AddScoped<PosBackend.Services.GeoLocationService>(provider =>
{
    var cacheService = provider.GetRequiredService<ICacheService>();
    var logger = provider.GetRequiredService<ILogger<PosBackend.Services.GeoLocationService>>();
    
    var dbPath = "GeoLite2-Country.mmdb";
    
    try
    {
        return new PosBackend.Services.GeoLocationService(dbPath, cacheService, logger);
    }
    catch
    {
        return new PosBackend.Services.GeoLocationServiceFallback(cacheService, logger);
    }
});

// Register new pricing services
builder.Services.AddScoped<PosBackend.Services.Interfaces.ICurrencyService, PosBackend.Services.CurrencyService>();
builder.Services.AddScoped<PosBackend.Services.Interfaces.ICurrencyDetectionService, PosBackend.Services.CurrencyDetectionService>();
builder.Services.AddScoped<PosBackend.Services.Interfaces.IPricingService, PosBackend.Services.PricingService>();

builder.Services.AddSignalR();

builder.Services.AddHttpClient();

// Configure controllers and JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "POS API", 
        Version = "v1",
        Description = "API for the POS System"
    });
    
    // Add a document filter to ensure OpenAPI version is set
    c.DocumentFilter<OpenApiVersionFilter>();
    
    // Ensure all APIs are included
    c.DocInclusionPredicate((docName, apiDesc) => true);
    
    // Add JWT authentication support in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Add API Key authentication support in Swagger UI
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key for system-level access. Enter your API key in the text input below.",
        Name = SecurityConstants.ApiKeys.HeaderName,
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Serilog is already configured above and will handle all logging


// Enhanced Response Compression Configuration
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    
    // MIME types to compress
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/json",
        "application/xml",
        "text/xml",
        "text/json",
        "text/plain",
        "text/css",
        "text/html",
        "application/javascript",
        "text/javascript",
        "image/svg+xml"
    });
});

// Configure compression levels
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

// Enhanced Rate Limiting Configuration
builder.Services.AddRateLimiter(options =>
{
    // Default policy for authenticated users
    options.AddPolicy(SecurityConstants.RateLimiting.AuthenticatedUserPolicy, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            }));

    // Stricter policy for anonymous users
    options.AddPolicy(SecurityConstants.RateLimiting.AnonymousUserPolicy, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2
            }));

    // Admin users get higher limits
    options.AddPolicy(SecurityConstants.RateLimiting.AdminUserPolicy, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? "admin",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 500,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 50
            }));

    // Health check endpoints get minimal limits
    options.AddPolicy(SecurityConstants.RateLimiting.HealthCheckPolicy, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "health",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 1
            }));

    // Global fallback
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var isAuthenticated = httpContext.User.Identity?.IsAuthenticated == true;
        var isAdmin = httpContext.User.IsInRole(SecurityConstants.Roles.Admin) || 
                     httpContext.User.IsInRole(SecurityConstants.Roles.SystemAdmin);

        var key = isAuthenticated 
            ? httpContext.User.Identity?.Name ?? "authenticated"
            : httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

        var limits = isAdmin ? (1000, 60) : isAuthenticated ? (200, 60) : (50, 60);

        return RateLimitPartition.GetFixedWindowLimiter(key, _ => 
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = limits.Item1,
                Window = TimeSpan.FromSeconds(limits.Item2)
            });
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        
        var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
        logger?.LogWarning("Rate limit exceeded for {UserName} from IP {RemoteIpAddress}", 
            context.HttpContext.User.Identity?.Name ?? "Anonymous",
            context.HttpContext.Connection.RemoteIpAddress);

        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
    };
});

var app = builder.Build();

// Use CORS early in the pipeline
app.UseCors("DefaultPolicy");

// Add security middleware (includes security headers)
app.UseSecurityMiddleware();

// Add input sanitization middleware
app.UseInputSanitization();

// Add rate limiting
app.UseRateLimiter();

// Add security headers
app.UseSecurityHeaders();

// Enable session before auth and endpoints
app.UseSession();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Enable response compression
app.UseResponseCompression();

// Add response caching middleware
app.UseMiddleware<ResponseCachingMiddleware>();

// Enable serving static files
app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
// CORS configuration is already applied above

// We're using SecurityHeadersMiddleware for security headers
// No need to set them here as well

// Use exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enable response compression

// Add cache control headers middleware
app.Use(async (context, next) =>
{
    // Add cache control headers for static files
    if (context.Request.Path.StartsWithSegments("/static") ||
        context.Request.Path.Value?.EndsWith(".js") == true ||
        context.Request.Path.Value?.EndsWith(".css") == true)
    {
        context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
    }

    await next();
});

app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
    // Add CORS headers to Swagger JSON endpoint
    c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
    {
        swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}" } };
    });
});

// We're using a custom Swagger UI implementation instead of the default one
// This is configured in wwwroot/swagger/index.html
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "POS API v1");
    c.RoutePrefix = "swagger";

    // These settings won't affect our custom UI, but we'll keep them for reference
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    c.DefaultModelsExpandDepth(-1);
    c.DisplayRequestDuration();
    c.EnableFilter();
    c.EnableDeepLinking();
    c.EnableValidator();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database exists and apply migrations, then seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<PosDbContext>();
        
        // Ensure database is created and apply migrations if needed
        Console.WriteLine("Ensuring database exists and applying migrations...");
        
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (connectionString == "InMemory")
        {
            // For InMemory database, just ensure it's created
            context.Database.EnsureCreated();
            Console.WriteLine("InMemory database created successfully.");
        }
        else
        {
            try
            {
                // Apply any pending migrations (this will create the database if it doesn't exist)
                var pendingMigrations = context.Database.GetPendingMigrations();
                if (pendingMigrations.Any())
                {
                    Console.WriteLine($"Applying {pendingMigrations.Count()} pending migrations...");
                    context.Database.Migrate();
                    Console.WriteLine("Migrations applied successfully.");
                }
                else
                {
                    Console.WriteLine("Database is up to date.");
                }
            }
            catch (Exception migrationEx)
            {
                Console.WriteLine($"Migration failed: {migrationEx.Message}");
                Console.WriteLine("Attempting to create database and apply migrations...");
                
                // If migrations fail, try to ensure database is created first
                context.Database.EnsureCreated();
                Console.WriteLine("Database created successfully.");
            }
        }

        // Seed package tiers first
        await PackageTierSeeder.SeedPackageTiers(context);

        // Check if there are any pricing packages
        var existingPackages = context.PricingPackages.Include(p => p.Prices).ToList();
        if (!existingPackages.Any())
        {
            Console.WriteLine("No pricing packages found. Seeding initial data...");

            var packages = new List<PricingPackage>
            {
                new PricingPackage
                {
                    Title = "Starter Plus",
                    Description = "Essential features for small businesses;Basic POS functionality;Single store support;Email support;Basic reporting;Customer database;Simple analytics",
                    Icon = "MUI:StarterIcon",
                    ExtraDescription = "Perfect for new businesses starting their POS journey",
                    TestPeriodDays = 14,
                    Type = "starter-plus",
                    TierLevel = 1
                },
                new PricingPackage
                {
                    Title = "Growth Pro",
                    Description = "Advanced features for growing businesses;Everything in Starter;Advanced inventory;Loyalty program;Marketing automation;Staff tracking;Custom dashboards;Mobile app",
                    Icon = "MUI:GrowthIcon",
                    ExtraDescription = "Designed for expanding businesses with growing needs",
                    TestPeriodDays = 14,
                    Type = "growth-pro",
                    TierLevel = 2
                },
                new PricingPackage
                {
                    Title = "Custom Pro",
                    Description = "Flexible solutions tailored to unique requirements;Customizable features;Industry specific;Flexible scaling;Personalized onboarding;Custom workflows;Advanced integrations;Priority support",
                    Icon = "MUI:CustomIcon",
                    ExtraDescription = "Tailored solutions for unique business requirements",
                    TestPeriodDays = 14,
                    Type = "custom-pro",
                    TierLevel = 3
                },
                new PricingPackage
                {
                    Title = "Enterprise Elite",
                    Description = "Comprehensive solutions for large organizations;All features included;Multi-location management;Enterprise analytics;Custom API integrations;White-label options;Dedicated account manager;Priority 24/7 support",
                    Icon = "MUI:EnterpriseIcon",
                    ExtraDescription = "Complete enterprise-grade POS solution",
                    TestPeriodDays = 30,
                    Type = "enterprise-elite",
                    TierLevel = 4
                },
                new PricingPackage
                {
                    Title = "Premium Plus",
                    Description = "Ultimate POS experience with cutting-edge AI;Everything in Enterprise;AI-powered analytics;Predictive inventory;Omnichannel integration;VIP support;Quarterly reviews;Custom reporting;Advanced AI insights",
                    Icon = "MUI:PremiumIcon",
                    ExtraDescription = "The ultimate POS experience with AI and premium services",
                    TestPeriodDays = 30,
                    Type = "premium-plus",
                    TierLevel = 5
                }
            };

            context.PricingPackages.AddRange(packages);
            context.SaveChanges();
            Console.WriteLine($"Added {packages.Count} pricing packages.");

            // Add pricing for each package in multiple currencies
            var packagePrices = new Dictionary<string, Dictionary<string, decimal>>
            {
                { "starter-plus", new Dictionary<string, decimal> { { "USD", 29.99m }, { "EUR", 27.99m }, { "GBP", 23.99m }, { "ZAR", 549.99m } } },
                { "growth-pro", new Dictionary<string, decimal> { { "USD", 79.99m }, { "EUR", 74.99m }, { "GBP", 63.99m }, { "ZAR", 1479.99m } } },
                { "custom-pro", new Dictionary<string, decimal> { { "USD", 149.99m }, { "EUR", 139.99m }, { "GBP", 119.99m }, { "ZAR", 2749.99m } } },
                { "enterprise-elite", new Dictionary<string, decimal> { { "USD", 249.99m }, { "EUR", 229.99m }, { "GBP", 199.99m }, { "ZAR", 4599.99m } } },
                { "premium-plus", new Dictionary<string, decimal> { { "USD", 399.99m }, { "EUR", 369.99m }, { "GBP", 319.99m }, { "ZAR", 7399.99m } } }
            };

            foreach (var package in packages)
            {
                if (packagePrices.ContainsKey(package.Type))
                {
                    foreach (var priceData in packagePrices[package.Type])
                    {
                        package.SetPrice(priceData.Value, priceData.Key);
                    }
                }
            }
            
            context.SaveChanges();
            Console.WriteLine("Added pricing data for all packages.");
        }
        else
        {
            Console.WriteLine("Pricing packages already exist in the database.");
            
            // Check if existing packages have proper pricing
            var packagesWithoutPrices = existingPackages.Where(p => !p.Prices.Any()).ToList();
            if (packagesWithoutPrices.Any())
            {
                Console.WriteLine($"Found {packagesWithoutPrices.Count} packages without pricing. Adding prices...");
                
                var packagePrices = new Dictionary<string, Dictionary<string, decimal>>
                {
                    { "starter-plus", new Dictionary<string, decimal> { { "USD", 29.99m }, { "EUR", 27.99m }, { "GBP", 23.99m }, { "ZAR", 549.99m } } },
                    { "growth-pro", new Dictionary<string, decimal> { { "USD", 79.99m }, { "EUR", 74.99m }, { "GBP", 63.99m }, { "ZAR", 1479.99m } } },
                    { "custom-pro", new Dictionary<string, decimal> { { "USD", 149.99m }, { "EUR", 139.99m }, { "GBP", 119.99m }, { "ZAR", 2749.99m } } },
                    { "enterprise-elite", new Dictionary<string, decimal> { { "USD", 249.99m }, { "EUR", 229.99m }, { "GBP", 199.99m }, { "ZAR", 4599.99m } } },
                    { "premium-plus", new Dictionary<string, decimal> { { "USD", 399.99m }, { "EUR", 369.99m }, { "GBP", 319.99m }, { "ZAR", 7399.99m } } }
                };

                foreach (var package in packagesWithoutPrices)
                {
                    if (packagePrices.ContainsKey(package.Type))
                    {
                        foreach (var priceData in packagePrices[package.Type])
                        {
                            package.SetPrice(priceData.Value, priceData.Key);
                        }
                        Console.WriteLine($"Added pricing for {package.Type}");
                    }
                }
                
                context.SaveChanges();
                Console.WriteLine("Finished adding pricing data to existing packages.");
            }
            else
            {
                Console.WriteLine("All packages already have pricing data.");
            }
        }

        if (!context.CoreFeatures.Any())
        {
            var features = new List<Feature>
            {
                new Feature { Name = "Point of Sale", Description = "Basic POS functionality with sales processing", BasePrice = 10.00m, IsRequired = true },
                new Feature { Name = "Inventory Management", Description = "Track and manage inventory levels", BasePrice = 5.00m, IsRequired = true },
                new Feature { Name = "Customer Management", Description = "Manage customer information and purchase history", BasePrice = 5.00m, IsRequired = false },
                new Feature { Name = "Reporting", Description = "Basic sales and inventory reports", BasePrice = 5.00m, IsRequired = false },
                new Feature { Name = "Employee Management", Description = "Manage employee accounts and permissions", BasePrice = 7.50m, IsRequired = false },
                new Feature { Name = "Multi-store Support", Description = "Support for multiple store locations", BasePrice = 15.00m, IsRequired = false },
                new Feature { Name = "Loyalty Program", Description = "Customer loyalty and rewards program", BasePrice = 10.00m, IsRequired = false },
                new Feature { Name = "E-commerce Integration", Description = "Integration with online store", BasePrice = 20.00m, IsRequired = false },
                new Feature { Name = "Mobile Access", Description = "Access POS from mobile devices", BasePrice = 7.50m, IsRequired = false },
                new Feature { Name = "Offline Mode", Description = "Continue operations when internet is down", BasePrice = 5.00m, IsRequired = false }
            };

            context.CoreFeatures.AddRange(features);
            context.SaveChanges();
            Console.WriteLine($"Added {features.Count} core features.");
        }

        // Add add-ons if they don't exist
        if (!context.AddOns.Any())
        {
            var addOns = new List<AddOn>
            {
                new AddOn { Name = "Advanced Analytics", Description = "Detailed business analytics and insights", Price = 15.00m },
                new AddOn { Name = "API Access", Description = "Access to API for custom integrations", Price = 25.00m },
                new AddOn { Name = "Custom Branding", Description = "White-label solution with your branding", Price = 20.00m },
                new AddOn { Name = "24/7 Support", Description = "Round-the-clock customer support", Price = 30.00m },
                new AddOn { Name = "Data Migration", Description = "Assistance with data migration from other systems", Price = 50.00m },
                new AddOn { Name = "Training Sessions", Description = "Personalized training for your team", Price = 40.00m },
                new AddOn { Name = "Hardware Bundle", Description = "Compatible POS hardware package", Price = 200.00m },
                new AddOn { Name = "Advanced Security", Description = "Enhanced security features and compliance", Price = 15.00m },
                new AddOn { Name = "Multi-currency Support", Description = "Support for multiple currencies", Price = 10.00m },
                new AddOn { Name = "Accounting Integration", Description = "Integration with popular accounting software", Price = 15.00m }
            };

            context.AddOns.AddRange(addOns);
            context.SaveChanges();
            Console.WriteLine($"Added {addOns.Count} add-ons.");
        }

        // Create default user subscription if it doesn't exist
        if (!context.UserSubscriptions.Any(us => us.UserId == "default-user"))
        {
            Console.WriteLine("Creating default user subscription...");
            
            // Get the first available pricing package (should be Starter Plus)
            var defaultPackage = await context.PricingPackages.FirstOrDefaultAsync();
            if (defaultPackage != null)
            {
                var defaultSubscription = new UserSubscription
                {
                    UserId = "default-user",
                    PricingPackageId = defaultPackage.Id,
                    StartDate = DateTime.UtcNow,
                    IsActive = true,
                    Status = "active",
                    EnabledFeatures = new List<string>
                    {
                        "Dashboard",
                        "Products List",
                        "Add/Edit Product",
                        "Sales Reports",
                        "Inventory Management",
                        "Customer Management"
                    },
                    AdditionalPackages = new List<int>(),
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                };

                context.UserSubscriptions.Add(defaultSubscription);
                await context.SaveChangesAsync();
                Console.WriteLine("Default user subscription created successfully.");
            }
            else
            {
                Console.WriteLine("No pricing packages found. Cannot create default subscription.");
            }
        }
        else
        {
            Console.WriteLine("Default user subscription already exists.");
        }

        // Update pricing packages with tier information
        await PackageTierSeeder.UpdatePricingPackagesWithTiers(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
    }
}

try
{
    Log.Information("Starting POS Backend application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class public for testing
public partial class Program { }
