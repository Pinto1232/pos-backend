using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using PosBackend.Application.Interfaces;
using PosBackend.Application.Services;
using PosBackend.Application.Services.Caching;
using PosBackend.Filters;
using PosBackend.Middlewares;
using PosBackend.Models;
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);
// Add JWT Bearer authentication for Keycloak
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
        options.RequireHttpsMetadata = true; // Set to false only for local dev with HTTP
    });


// Load environment variables
DotEnv.Load();

// Register PosDbContext for EF Core and Identity
builder.Services.AddDbContext<PosDbContext>(options =>
{
    // Use Npgsql for PostgreSQL, or change to UseSqlServer/UseSqlite as needed
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
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

builder.Services.AddSignalR();

builder.Services.AddHttpClient();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Logging.AddConsole();
builder.Logging.AddDebug();


// Register response compression
builder.Services.AddResponseCompression();

var app = builder.Build();

// Use CORS early in the pipeline
app.UseCors("DefaultPolicy");

// Add security headers
app.UseSecurityHeaders();

// Enable session before auth and endpoints
app.UseSession();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Enable response compression
app.UseResponseCompression();

// Enable serving static files
app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowAll");

// Add security headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "no-referrer");
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self';");
    await next();
});

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

            var packages = new List<PricingPackage>
            {

            };

            context.PricingPackages.AddRange(packages);
            context.SaveChanges();
            Console.WriteLine($"Added {packages.Count} pricing packages.");
        }
        else
        {
            Console.WriteLine("Pricing packages already exist in the database.");
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
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
    }
}

app.Run();

// Make Program class public for testing
public partial class Program { }
