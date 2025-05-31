using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PosBackend.Application.Services.Caching;
using PosBackend.Models;
using AppCacheKeys = PosBackend.Application.Services.Caching.CacheKeys;

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
                "starter-plus", new List<string>
                {
                    "Dashboard",
                    "Pricing Packages",
                    "Products List",
                    "Add/Edit Product",
                    "Product Categories",
                    "New Sale",
                    "Sales History",
                    "Invoices & Receipts",
                    "Customer List",
                    "Add/Edit Customer",
                    "Sales Reports",
                    "Stock Movement Report"
                }
            },
            {
                "growth-pro", new List<string>
                {
                    "Dashboard",
                    "Pricing Packages",
                    "Products List",
                    "Add/Edit Product",
                    "Product Categories",
                    "Stock Levels & Alerts",
                    "Low Stock Warnings",
                    "Inventory Adjustments",
                    "New Sale",
                    "Sales History",
                    "Invoices & Receipts",
                    "Returns & Refunds",
                    "Discounts & Promotions",
                    "Pending Orders",
                    "Completed Orders",
                    "Customer List",
                    "Add/Edit Customer",
                    "Customer Groups",
                    "Customer Purchase History",
                    "Supplier List",
                    "Add/Edit Supplier",
                    "Purchase Orders",
                    "Employee List",
                    "Roles & Permissions",
                    "Sales Reports",
                    "Top-Selling Products Report",
                    "Profit & Loss Report",
                    "Stock Movement Report",
                    "Employee Performance Report",
                    "Transaction History",
                    "Expense Tracking",
                    "Create New Discount",
                    "Active Promotions"
                }
            },
            {
                "custom-pro", new List<string>
                {
                    "Dashboard",
                    "Pricing Packages",
                    "Products List",
                    "Add/Edit Product",
                    "Product Categories",
                    "Stock Levels & Alerts",
                    "Low Stock Warnings",
                    "Bulk Import/Export",
                    "Inventory Adjustments",
                    "Product Expiry Tracking",
                    "New Sale",
                    "Sales History",
                    "Invoices & Receipts",
                    "Returns & Refunds",
                    "Discounts & Promotions",
                    "Loyalty & Reward Points",
                    "Pending Orders",
                    "Completed Orders",
                    "Cancelled Orders",
                    "Pre-Orders",
                    "Customer List",
                    "Add/Edit Customer",
                    "Customer Groups",
                    "Customer Purchase History",
                    "Loyalty Program",
                    "Customer Feedback & Reviews",
                    "Debt & Credit Management",
                    "Supplier List",
                    "Add/Edit Supplier",
                    "Purchase Orders",
                    "Pending Deliveries",
                    "Stock Replenishment Requests",
                    "Supplier Payments & Invoices",
                    "Employee List",
                    "Roles & Permissions",
                    "Cashier Sessions",
                    "Shift Management",
                    "Attendance Tracking",
                    "Activity Logs",
                    "Sales Reports",
                    "Top-Selling Products Report",
                    "Profit & Loss Report",
                    "Stock Movement Report",
                    "Employee Performance Report",
                    "Customer Purchase Trends Report",
                    "Tax & Compliance Reports",
                    "Payment Method Breakdown",
                    "Accepted Payment Methods ",
                    "Transaction History",
                    "Pending Payments",
                    "Refund Processing",
                    "Cash Management",
                    "Expense Tracking",
                    "Recurring Expenses",
                    "Cash Flow Overview",
                    "Supplier Payments",
                    "Tax Calculations",
                    "Create New Discount",
                    "Active Promotions",
                    "Coupon & Voucher Management",
                    "Seasonal & Flash Sales"
                }
            },
            {
                "premium-plus", new List<string>
                {
                    "Dashboard",
                    "Pricing Packages",
                    "Products List",
                    "Add/Edit Product",
                    "Product Categories",
                    "Stock Levels & Alerts",
                    "Low Stock Warnings",
                    "Bulk Import/Export",
                    "Inventory Adjustments",
                    "Product Expiry Tracking",
                    "New Sale",
                    "Sales History",
                    "Invoices & Receipts",
                    "Returns & Refunds",
                    "Discounts & Promotions",
                    "Loyalty & Reward Points",
                    "Pending Orders",
                    "Completed Orders",
                    "Cancelled Orders",
                    "Pre-Orders",
                    "Customer List",
                    "Add/Edit Customer",
                    "Customer Groups",
                    "Customer Purchase History",
                    "Loyalty Program",
                    "Customer Feedback & Reviews",
                    "Debt & Credit Management",
                    "Supplier List",
                    "Add/Edit Supplier",
                    "Purchase Orders",
                    "Pending Deliveries",
                    "Stock Replenishment Requests",
                    "Supplier Payments & Invoices",
                    "Employee List",
                    "Roles & Permissions",
                    "Cashier Sessions",
                    "Shift Management",
                    "Attendance Tracking",
                    "Activity Logs",
                    "Sales Reports",
                    "Top-Selling Products Report",
                    "Profit & Loss Report",
                    "Stock Movement Report",
                    "Employee Performance Report",
                    "Customer Purchase Trends Report",
                    "Tax & Compliance Reports",
                    "Payment Method Breakdown",
                    "Accepted Payment Methods ",
                    "Transaction History",
                    "Pending Payments",
                    "Refund Processing",
                    "Cash Management",
                    "Expense Tracking",
                    "Recurring Expenses",
                    "Cash Flow Overview",
                    "Supplier Payments",
                    "Tax Calculations",
                    "Create New Discount",
                    "Active Promotions",
                    "Coupon & Voucher Management",
                    "Seasonal & Flash Sales",
                    "Settings"
                }
            },
            {
                "enterprise-elite", new List<string>
                {
                    "Dashboard",
                    "Pricing Packages",
                    "Products List",
                    "Add/Edit Product",
                    "Product Categories",
                    "Stock Levels & Alerts",
                    "Low Stock Warnings",
                    "Bulk Import/Export",
                    "Inventory Adjustments",
                    "Product Expiry Tracking",
                    "New Sale",
                    "Sales History",
                    "Invoices & Receipts",
                    "Returns & Refunds",
                    "Discounts & Promotions",
                    "Loyalty & Reward Points",
                    "Pending Orders",
                    "Completed Orders",
                    "Cancelled Orders",
                    "Pre-Orders",
                    "Customer List",
                    "Add/Edit Customer",
                    "Customer Groups",
                    "Customer Purchase History",
                    "Loyalty Program",
                    "Customer Feedback & Reviews",
                    "Debt & Credit Management",
                    "Supplier List",
                    "Add/Edit Supplier",
                    "Purchase Orders",
                    "Pending Deliveries",
                    "Stock Replenishment Requests",
                    "Supplier Payments & Invoices",
                    "Employee List",
                    "Roles & Permissions",
                    "Cashier Sessions",
                    "Shift Management",
                    "Attendance Tracking",
                    "Activity Logs",
                    "Sales Reports",
                    "Top-Selling Products Report",
                    "Profit & Loss Report",
                    "Stock Movement Report",
                    "Employee Performance Report",
                    "Customer Purchase Trends Report",
                    "Tax & Compliance Reports",
                    "Payment Method Breakdown",
                    "Accepted Payment Methods ",
                    "Transaction History",
                    "Pending Payments",
                    "Refund Processing",
                    "Cash Management",
                    "Expense Tracking",
                    "Recurring Expenses",
                    "Cash Flow Overview",
                    "Supplier Payments",
                    "Tax Calculations",
                    "Create New Discount",
                    "Active Promotions",
                    "Coupon & Voucher Management",
                    "Seasonal & Flash Sales",
                    "Settings"
                }
            }
        };

        // Get features for a specific package type
        public List<string> GetFeaturesForPackage(string packageType)
        {
            // Create a cache key for this package type's features
            string cacheKey = AppCacheKeys.PackageFeatures(packageType);

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
            string cacheKey = AppCacheKeys.UserPackages(userId);

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
            string cacheKey = AppCacheKeys.UserFeatures(userId);

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
                await _cacheService.RemoveAsync(AppCacheKeys.UserFeatures(userId));
                await _cacheService.RemoveAsync(AppCacheKeys.UserPackages(userId));
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
                await _cacheService.RemoveAsync(AppCacheKeys.UserFeatures(userId));
                await _cacheService.RemoveAsync(AppCacheKeys.UserPackages(userId));
            }

            return true;
        }
    }
}
