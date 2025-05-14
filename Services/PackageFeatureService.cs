using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models;

namespace PosBackend.Services
{
    public class PackageFeatureService
    {
        private readonly PosDbContext _context;

        public PackageFeatureService(PosDbContext context)
        {
            _context = context;
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
            if (PackageFeatureMap.TryGetValue(packageType.ToLower(), out var features))
            {
                return features;
            }
            
            return new List<string>();
        }

        // Get user's active subscription
        public async Task<UserSubscription?> GetUserActiveSubscription(string userId)
        {
            return await _context.UserSubscriptions
                .Include(us => us.Package)
                .Where(us => us.UserId == userId && us.IsActive)
                .OrderByDescending(us => us.StartDate)
                .FirstOrDefaultAsync();
        }

        // Get all features available to a user based on their subscription and additional packages
        public async Task<List<string>> GetUserAvailableFeatures(string userId)
        {
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
            }

            return true;
        }
    }
}
