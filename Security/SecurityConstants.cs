namespace PosBackend.Security
{
    /// <summary>
    /// Security constants for authorization policies and roles
    /// </summary>
    public static class SecurityConstants
    {
        // Policy Names
        public static class Policies
        {
            public const string RequireAuthentication = "RequireAuthentication";
            public const string RequireAdmin = "RequireAdmin";
            public const string RequireUser = "RequireUser";
            public const string RequireSubscription = "RequireSubscription";
            public const string RequireValidSubscription = "RequireValidSubscription";
            public const string AllowAnonymousRead = "AllowAnonymousRead";
            public const string AllowAnonymousHealthCheck = "AllowAnonymousHealthCheck";
            public const string RequirePackageManagement = "RequirePackageManagement";
            public const string RequireSystemAdmin = "RequireSystemAdmin";
        }

        // Role Names
        public static class Roles
        {
            public const string Admin = "admin";
            public const string User = "user";
            public const string SystemAdmin = "system_admin";
            public const string PackageManager = "package_manager";
            public const string Subscriber = "subscriber";
        }

        // Claim Names
        public static class Claims
        {
            public const string UserId = "user_id";
            public const string Email = "email";
            public const string Role = "role";
            public const string Subscription = "subscription";
            public const string SubscriptionStatus = "subscription_status";
            public const string Permissions = "permissions";
        }

        // API Key Constants
        public static class ApiKeys
        {
            public const string HeaderName = "X-API-Key";
            public const string SystemApiKey = "POS_SYSTEM_API_KEY";
        }

        // Rate Limiting
        public static class RateLimiting
        {
            public const string AuthenticatedUserPolicy = "AuthenticatedUserPolicy";
            public const string AnonymousUserPolicy = "AnonymousUserPolicy";
            public const string AdminUserPolicy = "AdminUserPolicy";
            public const string HealthCheckPolicy = "HealthCheckPolicy";
        }
    }
}