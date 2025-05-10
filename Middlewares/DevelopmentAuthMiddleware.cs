using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace PosBackend.Middlewares
{
    public class DevelopmentAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;

        public DevelopmentAuthMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only apply in development environment
            if (_env.IsDevelopment())
            {
                // Check for development override header
                if (context.Request.Headers.ContainsKey("X-Dev-Override-Auth") &&
                    context.Request.Headers["X-Dev-Override-Auth"] == "true")
                {
                    // If the user is not authenticated, create a development identity
                    if (!context.User.Identity?.IsAuthenticated == true)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, "1"),
                            new Claim(ClaimTypes.Name, "dev-user"),
                            new Claim(ClaimTypes.Email, "dev@example.com"),
                            new Claim(ClaimTypes.Role, "Administrator")
                        };

                        var identity = new ClaimsIdentity(claims, "Development");
                        context.User = new ClaimsPrincipal(identity);
                    }
                }
            }

            await _next(context);
        }
    }

    // Extension method to add the middleware to the pipeline
    public static class DevelopmentAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseDevelopmentAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DevelopmentAuthMiddleware>();
        }
    }
}
