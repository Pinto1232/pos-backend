using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PosBackend.Application.Interfaces;
using PosBackend.Application.Services;
using PosBackend.Middlewares;
using PosBackend.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
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

builder.Services.AddDbContext<PosDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, UserRole>()
    .AddEntityFrameworkStores<PosDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserRepository, UserService>();

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

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "POS API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
