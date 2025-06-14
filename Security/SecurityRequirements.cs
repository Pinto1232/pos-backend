using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace PosBackend.Security
{
    /// <summary>
    /// Custom authorization requirements for specific business logic
    /// </summary>
    
    // Requirement for valid subscription
    public class ValidSubscriptionRequirement : IAuthorizationRequirement
    {
        public bool RequireActiveSubscription { get; }

        public ValidSubscriptionRequirement(bool requireActiveSubscription = true)
        {
            RequireActiveSubscription = requireActiveSubscription;
        }
    }

    // Requirement for package management operations
    public class PackageManagementRequirement : IAuthorizationRequirement
    {
        public string[] AllowedOperations { get; }

        public PackageManagementRequirement(params string[] allowedOperations)
        {
            AllowedOperations = allowedOperations;
        }
    }

    // Requirement for system administration
    public class SystemAdminRequirement : IAuthorizationRequirement
    {
        public string[] RequiredPermissions { get; }

        public SystemAdminRequirement(params string[] requiredPermissions)
        {
            RequiredPermissions = requiredPermissions;
        }
    }

    // Handler for valid subscription requirement
    public class ValidSubscriptionHandler : AuthorizationHandler<ValidSubscriptionRequirement>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ValidSubscriptionHandler> _logger;

        public ValidSubscriptionHandler(IServiceProvider serviceProvider, ILogger<ValidSubscriptionHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            ValidSubscriptionRequirement requirement)
        {
            var user = context.User;
            
            // Allow system admin to bypass subscription checks
            if (user.IsInRole(SecurityConstants.Roles.SystemAdmin))
            {
                context.Succeed(requirement);
                return;
            }

            // Get user ID from claims
            var userIdClaim = user.FindFirst(SecurityConstants.Claims.UserId) 
                             ?? user.FindFirst(ClaimTypes.NameIdentifier)
                             ?? user.FindFirst("sub");

            if (userIdClaim == null)
            {
                _logger.LogWarning("User ID not found in claims for subscription validation");
                context.Fail();
                return;
            }

            // Check subscription status
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var subscriptionService = scope.ServiceProvider.GetService<PosBackend.Services.SubscriptionService>();
                
                if (subscriptionService != null)
                {
                    var subscription = await subscriptionService.GetSubscriptionDetailsAsync(userIdClaim.Value);
                    
                    if (subscription != null && ((dynamic)subscription).IsActive)
                    {
                        _logger.LogDebug("Valid subscription found for user {UserId}", userIdClaim.Value);
                        context.Succeed(requirement);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating subscription for user {UserId}", userIdClaim.Value);
            }

            _logger.LogWarning("Valid subscription not found for user {UserId}", userIdClaim.Value);
            context.Fail();
        }
    }

    // Handler for package management requirement
    public class PackageManagementHandler : AuthorizationHandler<PackageManagementRequirement>
    {
        private readonly ILogger<PackageManagementHandler> _logger;

        public PackageManagementHandler(ILogger<PackageManagementHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            PackageManagementRequirement requirement)
        {
            var user = context.User;

            // Check if user has required roles
            if (user.IsInRole(SecurityConstants.Roles.SystemAdmin) || 
                user.IsInRole(SecurityConstants.Roles.PackageManager) ||
                user.IsInRole(SecurityConstants.Roles.Admin))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            _logger.LogWarning("Package management access denied for user {UserId}", 
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown");
            
            context.Fail();
            return Task.CompletedTask;
        }
    }

    // Handler for system admin requirement
    public class SystemAdminHandler : AuthorizationHandler<SystemAdminRequirement>
    {
        private readonly ILogger<SystemAdminHandler> _logger;

        public SystemAdminHandler(ILogger<SystemAdminHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            SystemAdminRequirement requirement)
        {
            var user = context.User;

            // Check if user has system admin role
            if (user.IsInRole(SecurityConstants.Roles.SystemAdmin))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            _logger.LogWarning("System admin access denied for user {UserId}", 
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown");
            
            context.Fail();
            return Task.CompletedTask;
        }
    }
}