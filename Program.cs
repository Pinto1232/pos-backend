using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PosBackend.Middlewares;
using PosBackend.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Configure Logging (Added Serilog for better logging)
builder.Host.UseSerilog((context, config) =>
{
    config.WriteTo.Console();
    config.WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day);
});

// 2️⃣ Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 3️⃣ Configure Swagger (Added OAuth2 integration for Keycloak)
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

// 4️⃣ Configure Database Connection
builder.Services.AddDbContext<PosDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 5️⃣ Configure JWT Authentication (Enhanced security & logging)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:ClientId"];
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Keycloak:Authority"],
            ValidateAudience = true,
            ValidAudiences = new[] { builder.Configuration["Keycloak:ClientId"], "realm-management", "broker", "account" },
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Error($"❌ Authentication failed: {context.Exception.Message}");
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync($"{{\"error\": \"Authentication failed\", \"message\": \"{context.Exception.Message}\"}}");
            },

            OnForbidden = context =>
            {
                Log.Warning("⛔ Access forbidden.");
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"error\": \"Forbidden\", \"message\": \"You do not have permission.\"}");
            },

            OnTokenValidated = context =>
            {
                Log.Information("✅ Token validated successfully.");
                return Task.CompletedTask;
            }
        };
    });

// 6️⃣ Enable Authorization
builder.Services.AddAuthorization();

// 7️⃣ Enable Controllers
builder.Services.AddControllers();

// 8️⃣ Build Application
var app = builder.Build();

// 9️⃣ Global Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 🔟 Enable Swagger in Development Mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "POS API v1");
        c.RoutePrefix = "swagger";
    });
}

// 1️⃣1️⃣ Enable CORS before Authentication
app.UseCors("DevPolicy");

// 1️⃣2️⃣ Enable Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 1️⃣3️⃣ Map Controllers
app.MapControllers();

// 1️⃣4️⃣ Run Application
try
{
    Log.Information("🚀 Starting POS API...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Application failed to start!");
}
finally
{
    Log.CloseAndFlush();
}
