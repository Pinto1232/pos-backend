using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PosBackend.Application.Interfaces;
using PosBackend.Application.Services;
using PosBackend.Data;
using PosBackend.Middlewares;
using PosBackend.Models;
using PosBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add memory cache for response caching
builder.Services.AddMemoryCache();

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
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true); // This is more permissive for development
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "POS API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] then your JWT token."
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
            new string[] {}
        }
    });
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

    // Enable sensitive data logging only in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }

    // Disable change tracking for read-only operations
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.AddIdentity<User, UserRole>()
    .AddEntityFrameworkStores<PosDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserRepository, UserService>();
builder.Services.AddScoped<IRoleRepository, RoleService>();
builder.Services.AddScoped<IPermissionRepository, PermissionService>();
builder.Services.AddScoped<IKeycloakService, KeycloakService>();
builder.Services.AddScoped<IRoleMappingService, RoleMappingService>();

// Register GeoLocation service
string geoLiteDbPath = Path.Combine(AppContext.BaseDirectory, "GeoLite2-Country.mmdb");
if (File.Exists(geoLiteDbPath))
{
    builder.Services.AddSingleton<GeoLocationService>(new GeoLocationService(geoLiteDbPath));
}
else
{
    builder.Services.AddSingleton<GeoLocationService, GeoLocationServiceFallback>();
}

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
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
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

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "POS API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseDevelopmentAuth(); // Add development auth middleware
app.UseAuthorization();
app.UsePermissionAuthorization();

app.MapControllers();

// Seed the database
await SeedData.Initialize(app.Services);

app.Run();
