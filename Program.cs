using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PosBackend.Models;

var builder = WebApplication.CreateBuilder(args);

// 1) CORS policy
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

// 2) Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "POS API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token.",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
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
});

// 3) EF Core with PostgreSQL (adjust your connection string in appsettings.json)
builder.Services.AddDbContext<PosDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4) Configure Keycloak + JWT Bearer Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // For Keycloak 19+, the realm path is "http://localhost:8080/realms/<realm>"
    options.Authority = "http://localhost:8280/realms/pisval-pos-realm";

    // The default audience in many Keycloak realms is "account".
    // If your tokens show a different "aud", put that here instead.
    options.Audience = "account";
    options.RequireHttpsMetadata = false;

    // Additional token validation settings
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Example: enforce that the token's "iss" claim must match the below
        ValidateIssuer = true,
        ValidIssuer = "http://localhost:8280/realms/pisval-pos-realm",
        ValidateAudience = true,
        ValidAudience = "account",
        ValidateLifetime = true
    };
});

// 5) Add Authorization
builder.Services.AddAuthorization();

// 5.5) IMPORTANT: Add Controllers!
builder.Services.AddControllers();

// 6) Build the WebApplication
var app = builder.Build();

// 7) Swagger + environment check
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    // Serve swagger at /swagger
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "POS API v1");
        c.RoutePrefix = "swagger"; // So your UI is at http://localhost:5107/swagger/index.html
    });
}

// Optional: If you want pure HTTP on port 5107, comment out if it causes forced HTTPS
// app.UseHttpsRedirection();

app.UseRouting();

// 8) Enable CORS before authentication/authorization
app.UseCors("DevPolicy");

// 9) Set up the authentication + authorization middlewares
app.UseAuthentication();
app.UseAuthorization();

// 10) Map controllers
app.MapControllers();

// 11) Run the app
app.Run();
