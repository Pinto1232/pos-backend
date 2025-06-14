using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace PosBackend.Middlewares
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Add security headers
            var headers = context.Response.Headers;

            // Prevent XSS attacks
            headers["X-XSS-Protection"] = "1; mode=block";

            // Prevent MIME type sniffing
            headers["X-Content-Type-Options"] = "nosniff";

            // Control iframe embedding
            headers["X-Frame-Options"] = "DENY";

            // Enable HSTS
            if (context.Request.IsHttps)
            {
                headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            }

            // Set referrer policy
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Set Content Security Policy
            // Check if the request is for Swagger UI
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                // More permissive CSP for Swagger UI
                headers["Content-Security-Policy"] = 
                    "default-src 'self' https://unpkg.com https://fonts.googleapis.com https://fonts.gstatic.com; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://unpkg.com; " +
                    "style-src 'self' 'unsafe-inline' https://unpkg.com https://fonts.googleapis.com; " +
                    "img-src 'self' data: https:; " +
                    "font-src 'self' data: https://fonts.gstatic.com; " +
                    "connect-src 'self' https://api.stripe.com https://unpkg.com; " +
                    "frame-ancestors 'none'; " +
                    "form-action 'self'; " +
                    "base-uri 'self';";
            }
            else
            {
                // Regular CSP for other pages
                headers["Content-Security-Policy"] = 
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                    "style-src 'self' 'unsafe-inline'; " +
                    "img-src 'self' data: https:; " +
                    "font-src 'self'; " +
                    "connect-src 'self' https://api.stripe.com; " +
                    "frame-ancestors 'none'; " +
                    "form-action 'self'; " +
                    "base-uri 'self';";
            }

            await _next(context);
        }
    }

    public static class SecurityHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}
