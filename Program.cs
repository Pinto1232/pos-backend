using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PosBackend.Middlewares;
using PosBackend.Models;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Configure CORS
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

// 2️⃣ Configure Swagger
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

// 3️⃣ Configure Database
builder.Services.AddDbContext<PosDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4️⃣ Configure HttpClientFactory
builder.Services.AddHttpClient(); // ✅ Ensures IHttpClientFactory is available

// 5️⃣ Configure JWT Authentication (Fixed Keycloak Authority Issue)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        string? keycloakAuthority = builder.Configuration["Keycloak:Authority"];
        string? keycloakClientId = builder.Configuration["Keycloak:ClientId"];

        if (string.IsNullOrEmpty(keycloakAuthority) || string.IsNullOrEmpty(keycloakClientId))
        {
            throw new Exception("⚠️ Keycloak settings are missing in appsettings.json!");
        }

        options.Authority = keycloakAuthority;
        options.Audience = keycloakClientId;
        options.RequireHttpsMetadata = false; // ⚠️ Important for local development

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = keycloakAuthority, // ✅ Match Keycloak URL
            ValidateAudience = true,
            ValidAudiences = new[] { keycloakClientId, "realm-management", "broker", "account" },
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"❌ Authentication failed: {context.Exception.Message}");
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync($"{{\"error\": \"Authentication failed\", \"message\": \"{context.Exception.Message}\"}}");
            },

            OnForbidden = context =>
            {
                Console.WriteLine("⛔ Access forbidden.");
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"error\": \"Forbidden\", \"message\": \"You do not have permission.\"}");
            },

            OnTokenValidated = context =>
            {
                Console.WriteLine("✅ Token validated successfully.");
                return Task.CompletedTask;
            }
        };
    });

// 6️⃣ Enable Authorization & Controllers
builder.Services.AddAuthorization();
builder.Services.AddControllers();

// 7️⃣ Build Application
var app = builder.Build();

// 8️⃣ Global Error Handling Middleware
//app.UseMiddleware<ExceptionHandlingMiddleware>();

// 9️⃣ Enable Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

// 🟢 Add your custom middleware AFTER framework handlers
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 1️⃣0️⃣ Enable Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "POS API v1");
    c.RoutePrefix = "swagger";
});

// 1️⃣1️⃣ Enable CORS before Authentication
app.UseCors("DevPolicy");

// 1️⃣2️⃣ Enable Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 1️⃣3️⃣ Map Controllers
app.MapControllers();

// 1️⃣4️⃣ Run Application
app.Run();
