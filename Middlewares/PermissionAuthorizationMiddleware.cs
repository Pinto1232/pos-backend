using Microsoft.AspNetCore.Http;
using PosBackend.Application.Interfaces;
using PosBackend.Utilities;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace PosBackend.Middlewares
{
    public class PermissionAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PermissionAuthorizationMiddleware> _logger;

        public PermissionAuthorizationMiddleware(RequestDelegate next, ILogger<PermissionAuthorizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IPermissionRepository permissionRepository)
        {
            // Skip authorization for non-API endpoints or if user is not authenticated
            if (!context.Request.Path.StartsWithSegments("/api") || !context.User.Identity?.IsAuthenticated == true)
            {
                await _next(context);
                return;
            }

            // Get the endpoint metadata
            var endpoint = context.GetEndpoint();
            if (endpoint == null)
            {
                await _next(context);
                return;
            }

            // Check for RequirePermission attributes
            var requirePermissionAttributes = endpoint.Metadata
                .OfType<RequirePermissionAttribute>()
                .ToList();

            // Check for RequireRole attributes
            var requireRoleAttributes = endpoint.Metadata
                .OfType<RequireRoleAttribute>()
                .ToList();

            // If no permission or role requirements, continue
            if (!requirePermissionAttributes.Any() && !requireRoleAttributes.Any())
            {
                await _next(context);
                return;
            }

            // Get user ID from claims
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("User ID claim not found in token");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            // Check Keycloak roles first if RequireRole attributes are present
            if (requireRoleAttributes.Any())
            {
                // Extract token from request
                var token = JwtTokenUtils.ExtractTokenFromRequest(context.Request);
                if (!string.IsNullOrEmpty(token))
                {
                    // Extract roles from token
                    var userRoles = JwtTokenUtils.ExtractRolesFromToken(token);

                    // Check if user has any of the required roles
                    foreach (var attribute in requireRoleAttributes)
                    {
                        if (userRoles.Contains(attribute.RoleName, StringComparer.OrdinalIgnoreCase))
                        {
                            _logger.LogInformation($"User has required Keycloak role: {attribute.RoleName}");
                            await _next(context);
                            return;
                        }
                    }
                }
            }

            // If we get here and there are no permission requirements, deny access
            if (!requirePermissionAttributes.Any())
            {
                _logger.LogWarning("User does not have any of the required Keycloak roles");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            // Check application permissions if RequirePermission attributes are present
            if (int.TryParse(userIdClaim.Value, out int userId))
            {
                // Check if user has any of the required permissions
                foreach (var attribute in requirePermissionAttributes)
                {
                    var hasPermission = await permissionRepository.HasPermissionAsync(userId, attribute.PermissionCode);
                    if (hasPermission)
                    {
                        _logger.LogInformation($"User has required permission: {attribute.PermissionCode}");
                        await _next(context);
                        return;
                    }
                }
            }
            else
            {
                _logger.LogWarning($"Could not parse user ID from claim: {userIdClaim.Value}");
            }

            // If we get here, the user doesn't have any of the required permissions or roles
            _logger.LogWarning("User does not have any of the required permissions");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
        }
    }

    // Extension method to add the middleware to the pipeline
    public static class PermissionAuthorizationMiddlewareExtensions
    {
        public static IApplicationBuilder UsePermissionAuthorization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PermissionAuthorizationMiddleware>();
        }
    }

    // Attribute to mark endpoints that require specific permissions
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class RequirePermissionAttribute : Attribute
    {
        public string PermissionCode { get; }

        public RequirePermissionAttribute(string permissionCode)
        {
            PermissionCode = permissionCode;
        }
    }

    // Attribute to mark endpoints that require specific Keycloak roles
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class RequireRoleAttribute : Attribute
    {
        public string RoleName { get; }

        public RequireRoleAttribute(string roleName)
        {
            RoleName = roleName;
        }
    }
}
