using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PosBackend.Application.Interfaces;
using PosBackend.Application.Services;
using PosBackend.Application.Services.Caching;
using PosBackend.Filters;
using PosBackend.Middlewares;
using PosBackend.Models;

var builder = WebApplication.CreateBuilder(args);

// Add memory cache for response caching and our cache service
builder.Services.AddMemoryCache();

// Configure and register the cache service
var cacheConfiguration = new CacheConfiguration();
builder.Configuration.GetSection("Cache").Bind(cacheConfiguration);
builder.Services.AddSingleton(cacheConfiguration);
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

// Add response compression for better performance
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = new[] {
        "text/plain",
        "text/css",
        "application/javascript",
        "text/html",
        "application/xml",
        "text/xml",
        "application/json",
        "text/json",
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",  // Next.js default port
                "http://localhost:3001",  // Alternative Next.js port
                "http://localhost:5107",  // Backend API
                "https://localhost:7005", // Backend API with HTTPS
                "http://localhost:8282"   // Keycloak
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true);
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "POS API",
        Version = "v1",
        Description = "API for Pisval Tech POS System",
        Contact = new OpenApiContact
        {
            Name = "Pisval Tech",
            Email = "support@pisvaltech.com"
        }
    });

    // Enable XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    else
    {
        Console.WriteLine($"⚠️ XML documentation file not found at: {xmlPath}");
    }

    // Add JWT Bearer Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
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
        }
    });

    // Configure operation filter to handle [Authorize] attribute
    c.OperationFilter<SecurityRequirementsOperationFilter>();
});

// Configure DbContext with performance optimizations
builder.Services.AddDbContext<PosDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            // Enable connection resiliency
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);

            // Set command timeout
            npgsqlOptions.CommandTimeout(30);
        });

    // Enable sensitive data logging only in development and if explicitly enabled in config
    if (builder.Environment.IsDevelopment() &&
        builder.Configuration.GetValue<bool>("EnableSensitiveDataLogging", false))
    {
        options.EnableSensitiveDataLogging();
    }

    // Disable change tracking for read-only operations
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

    // Suppress the pending model changes warning
    options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddIdentity<User, UserRole>()
    .AddEntityFrameworkStores<PosDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserRepository, UserService>();
builder.Services.AddScoped<PosBackend.Services.PackageFeatureService>();

builder.Services.AddSignalR();

builder.Services.AddHttpClient();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        string? keycloakAuthority = builder.Configuration["Keycloak:Authority"];
        string? keycloakClientId = builder.Configuration["Keycloak:ClientId"];

        if (string.IsNullOrEmpty(keycloakAuthority) || string.IsNullOrEmpty(keycloakClientId))
        {
            Console.WriteLine("⚠️ Keycloak settings are missing in appsettings.json! Exiting...");
            return;
        }

        options.Authority = keycloakAuthority;
        options.Audience = keycloakClientId;
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = keycloakAuthority,
            ValidateAudience = true,
            ValidAudiences = new[] { keycloakClientId, "realm-management", "broker", "account" },
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = async context =>
            {
                if (!context.Response.HasStarted)
                {
                    Console.WriteLine($"❌ Authentication failed: {context.Exception.Message}");
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\": \"Authentication failed\", \"message\": \"Invalid token\"}");
                }
            },

            OnForbidden = async context =>
            {
                if (!context.Response.HasStarted)
                {
                    Console.WriteLine("⛔ Access forbidden.");
                    context.Response.StatusCode = 403;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\": \"Forbidden\", \"message\": \"You do not have permission.\"}");
                }
            },

            OnTokenValidated = context =>
            {
                Console.WriteLine("✅ Token validated successfully.");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Use CORS early in the pipeline
app.UseCors("AllowAll");

// Use exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enable response compression
app.UseResponseCompression();

// Enable serving static files
app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Use built-in response caching middleware
app.UseResponseCaching();

// Use our custom response caching middleware
app.UseCustomResponseCaching();

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

// Seed pricing packages data if none exist
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<PosDbContext>();

        // Check if there are any pricing packages
        if (!context.PricingPackages.Any())
        {
            Console.WriteLine("No pricing packages found. Seeding initial data...");

            // Add pricing packages
            var packages = new List<PricingPackage>
            {
                new PricingPackage
                {
                    Title = "Starter",
                    Description = "Basic POS functionality;Inventory management;Single store support;Email support;Basic reporting",
                    Icon = "MUI:StarIcon",
                    ExtraDescription = "Perfect for small businesses just getting started",
                    Price = 29.99m,
                    TestPeriodDays = 14,
                    Type = "starter",
                    Currency = "USD",
                    MultiCurrencyPrices = "{\"ZAR\": 549.99, \"EUR\": 27.99, \"GBP\": 23.99}"
                },
                new PricingPackage
                {
                    Title = "Growth",
                    Description = "Everything in Starter;Multi-store support;Customer loyalty program;Priority support;Advanced reporting;Employee management",
                    Icon = "MUI:TrendingUpIcon",
                    ExtraDescription = "Ideal for growing businesses with multiple locations",
                    Price = 59.99m,
                    TestPeriodDays = 14,
                    Type = "growth",
                    Currency = "USD",
                    MultiCurrencyPrices = "{\"ZAR\": 999.99, \"EUR\": 54.99, \"GBP\": 47.99}"
                },
                new PricingPackage
                {
                    Title = "Premium",
                    Description = "Everything in Growth;Advanced inventory forecasting;Custom branding;24/7 support;API access;Advanced analytics;Multi-currency support",
                    Icon = "MUI:DiamondIcon",
                    ExtraDescription = "For established businesses requiring advanced features",
                    Price = 99.99m,
                    TestPeriodDays = 14,
                    Type = "premium",
                    Currency = "USD",
                    MultiCurrencyPrices = "{\"ZAR\": 1799.99, \"EUR\": 89.99, \"GBP\": 79.99}"
                },
                new PricingPackage
                {
                    Title = "Enterprise",
                    Description = "Everything in Premium;Dedicated account manager;Custom development;White-label solution;Unlimited users;Advanced security features;Data migration assistance",
                    Icon = "MUI:BusinessIcon",
                    ExtraDescription = "Tailored solutions for large enterprises",
                    Price = 199.99m,
                    TestPeriodDays = 30,
                    Type = "enterprise",
                    Currency = "USD",
                    MultiCurrencyPrices = "{\"ZAR\": 3499.99, \"EUR\": 179.99, \"GBP\": 159.99}"
                },
                new PricingPackage
                {
                    Title = "Custom",
                    Description = "Build your own package;Select only the features you need;Add modules as your business grows;Flexible pricing based on selections;Pay only for what you use;Scalable solution for any business size",
                    Icon = "MUI:SettingsIcon",
                    ExtraDescription = "Create a custom solution that fits your exact needs",
                    Price = 49.99m, // Updated price from 0.00m to 49.99m
                    TestPeriodDays = 14,
                    Type = "custom",
                    Currency = "USD",
                    MultiCurrencyPrices = "{\"ZAR\": 899.99, \"EUR\": 45.99, \"GBP\": 39.99}" // Updated prices
                }
            };

            context.PricingPackages.AddRange(packages);
            context.SaveChanges();
            Console.WriteLine($"Added {packages.Count} pricing packages.");
        }
        else
        {
            Console.WriteLine("Pricing packages already exist in the database.");

            // Update the Custom package price if it exists
            var customPackage = await context.PricingPackages
                .FirstOrDefaultAsync(p => p.Type == "custom");

            if (customPackage != null)
            {
                Console.WriteLine($"Found Custom package with current price: {customPackage.Price}");

                // Update the price
                customPackage.Price = 49.99m;
                customPackage.MultiCurrencyPrices = "{\"ZAR\": 899.99, \"EUR\": 45.99, \"GBP\": 39.99}";

                // Save changes
                await context.SaveChangesAsync();
                Console.WriteLine("Custom package price updated successfully to 49.99.");
            }
        }

        // Add core features if they don't exist
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
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
    }
}

app.Run();
