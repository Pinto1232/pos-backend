using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace PosBackend.Security
{
    /// <summary>
    /// Security middleware for additional security checks
    /// </summary>
    public class SecurityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public SecurityMiddleware(RequestDelegate next, ILogger<SecurityMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Add security headers
            AddSecurityHeaders(context);

            // Log security-relevant requests
            LogSecurityEvents(context);

            // Check for suspicious patterns
            if (IsSuspiciousRequest(context))
            {
                _logger.LogWarning("Suspicious request blocked from IP: {RemoteIpAddress}, Path: {Path}", 
                    context.Connection.RemoteIpAddress, context.Request.Path);
                
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden");
                return;
            }

            await _next(context);
        }

        private void AddSecurityHeaders(HttpContext context)
        {
            var response = context.Response;
            
            // Prevent clickjacking
            response.Headers["X-Frame-Options"] = "DENY";
            
            // Prevent MIME type sniffing
            response.Headers["X-Content-Type-Options"] = "nosniff";
            
            // XSS Protection
            response.Headers["X-XSS-Protection"] = "1; mode=block";
            
            // Referrer Policy
            response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            
            // Content Security Policy (adjust based on your needs)
            response.Headers["Content-Security-Policy"] = 
                "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:;";
            
            // Only add HSTS in production
            if (!context.Request.IsHttps && context.RequestServices.GetService<IWebHostEnvironment>()?.IsProduction() == true)
            {
                response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            }
        }

        private void LogSecurityEvents(HttpContext context)
        {
            var request = context.Request;
            var user = context.User;

            // Log authentication events
            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? user.FindFirst(SecurityConstants.Claims.UserId)?.Value;
                
                _logger.LogDebug("Authenticated request from user {UserId} to {Path} from IP {RemoteIpAddress}", 
                    userId, request.Path, context.Connection.RemoteIpAddress);
            }

            // Log sensitive endpoints
            var sensitivePaths = new[] { "/api/subscription", "/api/payment", "/api/admin" };
            if (sensitivePaths.Any(path => request.Path.StartsWithSegments(path)))
            {
                _logger.LogInformation("Access to sensitive endpoint {Path} from IP {RemoteIpAddress}", 
                    request.Path, context.Connection.RemoteIpAddress);
            }
        }

        private bool IsSuspiciousRequest(HttpContext context)
        {
            var request = context.Request;
            
            // Check for common attack patterns in URL
            var suspiciousPatterns = new[]
            {
                "../", "..\\", "<script", "javascript:", "vbscript:",
                "onload=", "onerror=", "eval(", "exec(", "union select",
                "drop table", "insert into", "update set", "delete from"
            };

            var path = request.Path.ToString().ToLowerInvariant();
            var query = request.QueryString.ToString().ToLowerInvariant();
            
            foreach (var pattern in suspiciousPatterns)
            {
                if (path.Contains(pattern) || query.Contains(pattern))
                {
                    return true;
                }
            }

            // Check request body for POST/PUT requests
            if (request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) || 
                request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
            {
                if (request.ContentLength > 10_000_000) // 10MB limit
                {
                    _logger.LogWarning("Request body too large: {ContentLength} bytes", request.ContentLength);
                    return true;
                }
            }

            // Check for excessive headers
            if (request.Headers.Count > 50)
            {
                _logger.LogWarning("Excessive headers in request: {HeaderCount}", request.Headers.Count);
                return true;
            }

            return false;
        }
    }

    public static class SecurityMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityMiddleware>();
        }
    }
}