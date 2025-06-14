using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosBackend.Security;
using System.Security.Claims;

namespace PosBackend.Controllers
{
    /// <summary>
    /// Base controller with security features
    /// </summary>
    [ApiController]
    [Authorize(Policy = SecurityConstants.Policies.RequireAuthentication)]
    public abstract class SecureBaseController : ControllerBase
    {
        /// <summary>
        /// Gets the current user ID from claims
        /// </summary>
        protected string? GetCurrentUserId()
        {
            return User.FindFirst(SecurityConstants.Claims.UserId)?.Value 
                   ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? User.FindFirst("sub")?.Value;
        }

        /// <summary>
        /// Gets the current user email from claims
        /// </summary>
        protected string? GetCurrentUserEmail()
        {
            return User.FindFirst(SecurityConstants.Claims.Email)?.Value 
                   ?? User.FindFirst(ClaimTypes.Email)?.Value
                   ?? User.FindFirst("email")?.Value;
        }

        /// <summary>
        /// Checks if the current user has a specific role
        /// </summary>
        protected bool HasRole(string role)
        {
            return User.IsInRole(role);
        }

        /// <summary>
        /// Checks if the current user is an admin
        /// </summary>
        protected bool IsAdmin()
        {
            return HasRole(SecurityConstants.Roles.Admin) || 
                   HasRole(SecurityConstants.Roles.SystemAdmin);
        }

        /// <summary>
        /// Checks if the current user is a system admin
        /// </summary>
        protected bool IsSystemAdmin()
        {
            return HasRole(SecurityConstants.Roles.SystemAdmin);
        }

        /// <summary>
        /// Gets the client IP address
        /// </summary>
        protected string? GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        /// <summary>
        /// Logs security-related actions
        /// </summary>
        protected void LogSecurityAction(string action, object? data = null)
        {
            var logger = HttpContext.RequestServices.GetService<ILogger<SecureBaseController>>();
            logger?.LogInformation("Security action: {Action} by user {UserId} from IP {RemoteIpAddress}. Data: {@Data}",
                action, GetCurrentUserId(), GetClientIpAddress(), data);
        }

        /// <summary>
        /// Creates a standardized error response
        /// </summary>
        protected IActionResult SecurityError(string message, int statusCode = 403)
        {
            LogSecurityAction($"Security error: {message}");
            return StatusCode(statusCode, new { error = "Security Error", message, timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Validates that the user can access a specific user's data
        /// </summary>
        protected bool CanAccessUserData(string targetUserId)
        {
            var currentUserId = GetCurrentUserId();
            
            // User can access their own data
            if (currentUserId == targetUserId)
                return true;

            // Admins can access any user's data
            if (IsAdmin())
                return true;

            return false;
        }

        /// <summary>
        /// Returns forbidden if user cannot access the requested data
        /// </summary>
        protected IActionResult? CheckUserDataAccess(string targetUserId)
        {
            if (!CanAccessUserData(targetUserId))
            {
                return SecurityError("Access denied: You can only access your own data");
            }
            return null;
        }
    }

    /// <summary>
    /// Base controller for admin-only operations
    /// </summary>
    [Authorize(Policy = SecurityConstants.Policies.RequireAdmin)]
    public abstract class AdminBaseController : SecureBaseController
    {
        protected AdminBaseController()
        {
            // Additional admin-specific initialization if needed
        }
    }

    /// <summary>
    /// Base controller for public read-only operations
    /// </summary>
    [Authorize(Policy = SecurityConstants.Policies.AllowAnonymousRead)]
    public abstract class PublicReadOnlyController : ControllerBase
    {
        /// <summary>
        /// Gets the client IP address for logging
        /// </summary>
        protected string? GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        /// <summary>
        /// Logs public access for monitoring
        /// </summary>
        protected void LogPublicAccess(string resource)
        {
            var logger = HttpContext.RequestServices.GetService<ILogger<PublicReadOnlyController>>();
            logger?.LogInformation("Public access to {Resource} from IP {RemoteIpAddress}",
                resource, GetClientIpAddress());
        }
    }
}