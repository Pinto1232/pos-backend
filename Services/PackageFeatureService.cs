using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PosBackend.Application.Services.Caching;
using PosBackend.Models;

namespace PosBackend.Services
{
    public class PackageFeatureService
    {
        private readonly PosDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly ILogger<PackageFeatureService> _logger;

        public PackageFeatureService(
            PosDbContext context,
            ICacheService cacheService,
            ILogger<PackageFeatureService> logger)
        {
            _context = context;
            _cacheService = cacheService;
            _logger = logger;
        }

        // Dictionary mapping package types to features
        private static readonly Dictionary<string, List<string>> PackageFeatureMap = new()
        {
            {
                "starter", new List<string>
                {
                    "Dashboard",
                    "Products List",
                    "Add/Edit Product",
                    "Sales Reports",
                    "Inventory Management",
                    "Customer Management"
                }
            },
            {
                "growth", new List<string>
                {
                    "Dashboard",
                    "Products List",
                    "Add/Edit Product",
                    "Sales Reports",
                    "Inventory Management",
                    "Customer Management",
                    "Supplier Management",
                    "Discount Management",
                    "Employee Management",
                    "Multi-User Access",
                    "Advanced Reporting"
                }
            },
            {
                "premium", new List<string>
                {
                    "Dashboard",
                    "Products List",
                    "Add/Edit Product",
                    "Sales Reports",
                    "Inventory Management",
                    "Customer Management",
                    "Supplier Management",
                    "Discount Management",
                    "Employee Management",
                    "Multi-User Access",
                    "Advanced Reporting",
                    "Multi-Store Management",
                    "API Access",
                    "Loyalty Program",
                    "Marketing Tools",
                    "E-commerce Integration"
                }
            },
            {
                "enterprise", new List<string>
                {
                    "Dashboard",
                    "Products List",
                    "Add/Edit Product",
                    "Sales Reports",
                    "Inventory Management",
                    "Customer Management",
                    "Supplier Management",
                    "Discount Management",
                    "Employee Management",
                    "Multi-User Access",
                    "Advanced Reporting",
                    "Multi-Store Management",
                    "API Access",
                    "Loyalty Program",
                    "Marketing Tools",
                    "E-commerce Integration",
                    "Enterprise Support",
                    "Custom Integrations",
                    "Advanced Analytics"
                }
            }
        };

        // Get features for a specific package type
        public List<string> GetFeaturesForPackage(string packageType)
        {
            // Create a cache key for this package type's features
            string cacheKey = CacheKeys.PackageFeatures(packageType);

            // Try to get from cache first
            return _cacheService.GetOrSet(cacheKey, () =>
            {
                _logger.LogDebug("Cache miss for package features: {PackageType}. Using static mapping.", packageType);

                if (PackageFeatureMap.TryGetValue(packageType.ToLower(), out var features))
                {
                    return features;
                }

                return new List<string>();
            });
        }

        // Get user's active subscription
        public async Task<UserSubscription?> GetUserActiveSubscription(string userId)
        {
            // Create a cache key for this user's subscription
            string cacheKey = CacheKeys.UserPackages(userId);

            // Try to get from cache first
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _logger.LogDebug("Cache miss for user subscription: {UserId}. Fetching from database.", userId);

                return await _context.UserSubscriptions
                    .Include(us => us.Package)
                    .Where(us => us.UserId == userId && us.IsActive)
                    .OrderByDescending(us => us.StartDate)
                    .FirstOrDefaultAsync();
            });
        }

        // Get all features available to a user based on their subscription and additional packages
        public async Task<List<string>> GetUserAvailableFeatures(string userId)
        {
            // Create a cache key for this user's features
            string cacheKey = CacheKeys.UserFeatures(userId);

            // Try to get from cache first
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _logger.LogDebug("Cache miss for user features: {UserId}. Calculating available features.", userId);

                var subscription = await GetUserActiveSubscription(userId);
                if (subscription == null || subscription.Package == null)
                {
                    return new List<string>();
                }

                var features = new HashSet<string>(GetFeaturesForPackage(subscription.Package.Type));

                // Add features from additional packages
                foreach (var packageId in subscription.AdditionalPackages)
                {
                    var additionalPackage = await _context.PricingPackages.FindAsync(packageId);
                    if (additionalPackage != null)
                    {
                        var additionalFeatures = GetFeaturesForPackage(additionalPackage.Type);
                        foreach (var feature in additionalFeatures)
                        {
                            features.Add(feature);
                        }
                    }
                }

                // Add any custom enabled features
                foreach (var feature in subscription.EnabledFeatures)
                {
                    features.Add(feature);
                }

                return features.ToList();
            });
        }

        // Check if a user has access to a specific feature
        public async Task<bool> UserHasFeatureAccess(string userId, string featureName)
        {
            var availableFeatures = await GetUserAvailableFeatures(userId);
            return availableFeatures.Contains(featureName);
        }

        // Enable an additional package for a user
        public async Task<bool> EnableAdditionalPackage(string userId, int packageId)
        {
            var subscription = await GetUserActiveSubscription(userId);
            if (subscription == null)
            {
                return false;
            }

            if (!subscription.AdditionalPackages.Contains(packageId))
            {
                subscription.AdditionalPackages.Add(packageId);
                subscription.LastUpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Invalidate cache for this user's features and packages
                await _cacheService.RemoveAsync(CacheKeys.UserFeatures(userId));
                await _cacheService.RemoveAsync(CacheKeys.UserPackages(userId));
            }

            return true;
        }

        // Disable an additional package for a user
        public async Task<bool> DisableAdditionalPackage(string userId, int packageId)
        {
            var subscription = await GetUserActiveSubscription(userId);
            if (subscription == null)
            {
                return false;
            }

            if (subscription.AdditionalPackages.Contains(packageId))
            {
                subscription.AdditionalPackages.Remove(packageId);
                subscription.LastUpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Invalidate cache for this user's features and packages
                await _cacheService.RemoveAsync(CacheKeys.UserFeatures(userId));
                await _cacheService.RemoveAsync(CacheKeys.UserPackages(userId));
            }

            return true;
        }
    }
}
