using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PosBackend.Models;
using System.Diagnostics;

namespace PosBackend.Services
{
    public interface IAdvancedPermissionService
    {
        Task<bool> HasFeatureAccessAsync(string userId, string featureName, Dictionary<string, object>? context = null);
        Task<FeatureAccessResult> EvaluateFeatureAccessAsync(string userId, string featureName, Dictionary<string, object>? context = null);
        Task<List<string>> GetUserAvailableFeaturesAsync(string userId);
        Task<bool> TrackFeatureUsageAsync(string userId, string featureName, Dictionary<string, object>? metadata = null);
        Task<FeatureUsageInfo> GetFeatureUsageInfoAsync(string userId, string featureName);
        Task<bool> CreateFeatureFlagAsync(FeatureFlag featureFlag);
        Task<bool> UpdateFeatureFlagAsync(int flagId, FeatureFlag updatedFlag);
        Task<bool> SetUserFeatureOverrideAsync(string userId, string featureName, bool isEnabled, string reason, DateTime? expiresAt = null);
        Task<List<FeatureFlag>> GetActiveFeatureFlagsAsync();
        Task InvalidateUserCacheAsync(string userId);
    }

    public class FeatureAccessResult
    {
        public bool HasAccess { get; set; }
        public string Reason { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
        public long EvaluationTimeMs { get; set; }
        public List<string> AppliedRules { get; set; } = new();
    }

    public class FeatureUsageInfo
    {
        public int CurrentUsage { get; set; }
        public int? UsageLimit { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public bool HasExceededLimit { get; set; }
        public int RemainingUsage { get; set; }
    }

    public class AdvancedPermissionService : IAdvancedPermissionService
    {
        private readonly PosDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AdvancedPermissionService> _logger;
        private readonly PackageFeatureService _packageFeatureService;
        private readonly Random _random = new();

        private const int CACHE_DURATION_MINUTES = 15;
        private const string CACHE_PREFIX_USER_FEATURES = "user_features_";
        private const string CACHE_PREFIX_FEATURE_FLAGS = "feature_flags_";
        private const string CACHE_PREFIX_USER_USAGE = "user_usage_";

        public AdvancedPermissionService(
            PosDbContext context,
            IMemoryCache cache,
            ILogger<AdvancedPermissionService> logger,
            PackageFeatureService packageFeatureService)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
            _packageFeatureService = packageFeatureService;
        }

        public async Task<bool> HasFeatureAccessAsync(string userId, string featureName, Dictionary<string, object>? context = null)
        {
            var result = await EvaluateFeatureAccessAsync(userId, featureName, context);
            return result.HasAccess;
        }

        public async Task<FeatureAccessResult> EvaluateFeatureAccessAsync(string userId, string featureName, Dictionary<string, object>? context = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new FeatureAccessResult();
            var appliedRules = new List<string>();

            try
            {
                _logger.LogDebug("Evaluating feature access for user {UserId}, feature {FeatureName}", userId, featureName);

                // Step 1: Check user-specific overrides first (highest priority)
                var userOverride = await GetUserFeatureOverrideAsync(userId, featureName);
                if (userOverride != null)
                {
                    appliedRules.Add($"User override: {userOverride.Reason}");
                    result.HasAccess = userOverride.IsEnabled;
                    result.Reason = $"User-specific override: {userOverride.Reason}";
                    result.AppliedRules = appliedRules;
                    await LogFeatureAccessAsync(userId, featureName, result.HasAccess, result.Reason, context, stopwatch.ElapsedMilliseconds);
                    return result;
                }

                // Step 2: Check feature flags
                var featureFlags = await GetFeatureFlagsForFeatureAsync(featureName);
                foreach (var flag in featureFlags.OrderByDescending(f => f.Priority))
                {
                    var flagResult = await EvaluateFeatureFlagAsync(userId, flag, context);
                    if (flagResult.HasValue)
                    {
                        appliedRules.Add($"Feature flag '{flag.Name}': {flagResult.Value}");
                        if (!flagResult.Value)
                        {
                            result.HasAccess = false;
                            result.Reason = $"Feature flag '{flag.Name}' denied access";
                            result.AppliedRules = appliedRules;
                            await LogFeatureAccessAsync(userId, featureName, result.HasAccess, result.Reason, context, stopwatch.ElapsedMilliseconds);
                            return result;
                        }
                    }
                }

                // Step 3: Check usage limitations
                var usageInfo = await GetFeatureUsageInfoAsync(userId, featureName);
                if (usageInfo.HasExceededLimit)
                {
                    appliedRules.Add($"Usage limit exceeded: {usageInfo.CurrentUsage}/{usageInfo.UsageLimit}");
                    result.HasAccess = false;
                    result.Reason = $"Usage limit exceeded for feature '{featureName}'";
                    result.AppliedRules = appliedRules;
                    await LogFeatureAccessAsync(userId, featureName, result.HasAccess, result.Reason, context, stopwatch.ElapsedMilliseconds);
                    return result;
                }

                // Step 4: Check package-based access (fallback to existing system)
                var hasPackageAccess = await _packageFeatureService.UserHasFeatureAccess(userId, featureName);
                appliedRules.Add($"Package-based access: {hasPackageAccess}");

                result.HasAccess = hasPackageAccess;
                result.Reason = hasPackageAccess ? "Access granted by subscription package" : "Feature not available in current package";
                result.AppliedRules = appliedRules;
                result.Metadata["usageInfo"] = usageInfo;

                await LogFeatureAccessAsync(userId, featureName, result.HasAccess, result.Reason, context, stopwatch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating feature access for user {UserId}, feature {FeatureName}", userId, featureName);
                result.HasAccess = false;
                result.Reason = "Error during feature access evaluation";
                return result;
            }
            finally
            {
                stopwatch.Stop();
                result.EvaluationTimeMs = stopwatch.ElapsedMilliseconds;
            }
        }

        public async Task<List<string>> GetUserAvailableFeaturesAsync(string userId)
        {
            var cacheKey = $"{CACHE_PREFIX_USER_FEATURES}{userId}";

            if (_cache.TryGetValue(cacheKey, out List<string>? cachedFeatures) && cachedFeatures != null)
            {
                return cachedFeatures;
            }

            var features = new List<string>();

            // Get package-based features
            var packageFeatures = await _packageFeatureService.GetUserAvailableFeatures(userId);
            features.AddRange(packageFeatures);

            // Get additional features from feature flags
            var featureFlags = await GetActiveFeatureFlagsAsync();
            foreach (var flag in featureFlags)
            {
                if (!features.Contains(flag.Name))
                {
                    var hasAccess = await HasFeatureAccessAsync(userId, flag.Name);
                    if (hasAccess)
                    {
                        features.Add(flag.Name);
                    }
                }
            }

            _cache.Set(cacheKey, features, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));
            return features;
        }

        public async Task<bool> TrackFeatureUsageAsync(string userId, string featureName, Dictionary<string, object>? metadata = null)
        {
            try
            {
                var now = DateTime.UtcNow;
                var usage = await _context.UserFeatureUsages
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.FeatureName == featureName && u.PeriodEndDate > now);

                if (usage == null)
                {
                    // Create new usage record
                    usage = new UserFeatureUsage
                    {
                        UserId = userId,
                        FeatureName = featureName,
                        UsageCount = 1,
                        LastUsedAt = now,
                        PeriodStartDate = now,
                        PeriodEndDate = GetPeriodEndDate(now, "monthly"), // Default to monthly
                        UsageMetadata = JsonConvert.SerializeObject(metadata ?? new Dictionary<string, object>())
                    };
                    _context.UserFeatureUsages.Add(usage);
                }
                else
                {
                    // Update existing usage
                    usage.UsageCount++;
                    usage.LastUsedAt = now;
                    usage.UpdatedAt = now;
                    if (metadata != null)
                    {
                        usage.UsageMetadata = JsonConvert.SerializeObject(metadata);
                    }
                }

                await _context.SaveChangesAsync();

                // Invalidate cache
                var cacheKey = $"{CACHE_PREFIX_USER_USAGE}{userId}_{featureName}";
                _cache.Remove(cacheKey);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking feature usage for user {UserId}, feature {FeatureName}", userId, featureName);
                return false;
            }
        }

        public async Task<FeatureUsageInfo> GetFeatureUsageInfoAsync(string userId, string featureName)
        {
            var cacheKey = $"{CACHE_PREFIX_USER_USAGE}{userId}_{featureName}";

            if (_cache.TryGetValue(cacheKey, out FeatureUsageInfo? cachedInfo) && cachedInfo != null)
            {
                return cachedInfo;
            }

            var now = DateTime.UtcNow;
            var usage = await _context.UserFeatureUsages
                .FirstOrDefaultAsync(u => u.UserId == userId && u.FeatureName == featureName && u.PeriodEndDate > now);

            var featureFlag = await _context.FeatureFlags
                .FirstOrDefaultAsync(f => f.Name == featureName && !f.IsDeleted);

            var info = new FeatureUsageInfo();

            if (usage != null)
            {
                info.CurrentUsage = usage.UsageCount;
                info.PeriodStartDate = usage.PeriodStartDate;
                info.PeriodEndDate = usage.PeriodEndDate;
            }
            else
            {
                info.CurrentUsage = 0;
                info.PeriodStartDate = now;
                info.PeriodEndDate = GetPeriodEndDate(now, "monthly");
            }

            if (featureFlag?.UsageLimit.HasValue == true)
            {
                info.UsageLimit = featureFlag.UsageLimit.Value;
                info.HasExceededLimit = info.CurrentUsage >= info.UsageLimit.Value;
                info.RemainingUsage = Math.Max(0, info.UsageLimit.Value - info.CurrentUsage);
            }
            else
            {
                info.HasExceededLimit = false;
                info.RemainingUsage = int.MaxValue;
            }

            _cache.Set(cacheKey, info, TimeSpan.FromMinutes(5));
            return info;
        }

        public async Task<bool> CreateFeatureFlagAsync(FeatureFlag featureFlag)
        {
            try
            {
                featureFlag.CreatedAt = DateTime.UtcNow;
                featureFlag.UpdatedAt = DateTime.UtcNow;

                _context.FeatureFlags.Add(featureFlag);
                await _context.SaveChangesAsync();

                // Invalidate relevant caches
                _cache.Remove(CACHE_PREFIX_FEATURE_FLAGS);

                _logger.LogInformation("Created feature flag {FeatureName}", featureFlag.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating feature flag {FeatureName}", featureFlag.Name);
                return false;
            }
        }

        public async Task<bool> UpdateFeatureFlagAsync(int flagId, FeatureFlag updatedFlag)
        {
            try
            {
                var existingFlag = await _context.FeatureFlags.FindAsync(flagId);
                if (existingFlag == null)
                {
                    return false;
                }

                existingFlag.Name = updatedFlag.Name;
                existingFlag.Description = updatedFlag.Description;
                existingFlag.IsEnabled = updatedFlag.IsEnabled;
                existingFlag.Type = updatedFlag.Type;
                existingFlag.Configuration = updatedFlag.Configuration;
                existingFlag.StartDate = updatedFlag.StartDate;
                existingFlag.EndDate = updatedFlag.EndDate;
                existingFlag.UsageLimit = updatedFlag.UsageLimit;
                existingFlag.UsagePeriod = updatedFlag.UsagePeriod;
                existingFlag.RolloutPercentage = updatedFlag.RolloutPercentage;
                existingFlag.TargetAudience = updatedFlag.TargetAudience;
                existingFlag.RequiredPackageTypes = updatedFlag.RequiredPackageTypes;
                existingFlag.MinimumPackageLevel = updatedFlag.MinimumPackageLevel;
                existingFlag.Priority = updatedFlag.Priority;
                existingFlag.UpdatedAt = DateTime.UtcNow;
                existingFlag.UpdatedBy = updatedFlag.UpdatedBy;

                await _context.SaveChangesAsync();

                // Invalidate relevant caches
                _cache.Remove(CACHE_PREFIX_FEATURE_FLAGS);

                _logger.LogInformation("Updated feature flag {FeatureName}", existingFlag.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating feature flag {FlagId}", flagId);
                return false;
            }
        }

        public async Task<bool> SetUserFeatureOverrideAsync(string userId, string featureName, bool isEnabled, string reason, DateTime? expiresAt = null)
        {
            try
            {
                var featureFlag = await _context.FeatureFlags
                    .FirstOrDefaultAsync(f => f.Name == featureName && !f.IsDeleted);

                if (featureFlag == null)
                {
                    _logger.LogWarning("Feature flag not found for feature {FeatureName}", featureName);
                    return false;
                }

                var existingOverride = await _context.FeatureFlagOverrides
                    .FirstOrDefaultAsync(o => o.FeatureFlagId == featureFlag.Id && o.UserId == userId);

                if (existingOverride != null)
                {
                    existingOverride.IsEnabled = isEnabled;
                    existingOverride.Reason = reason;
                    existingOverride.OverrideEndDate = expiresAt;
                    existingOverride.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    var newOverride = new FeatureFlagOverride
                    {
                        FeatureFlagId = featureFlag.Id,
                        UserId = userId,
                        IsEnabled = isEnabled,
                        Reason = reason,
                        OverrideStartDate = DateTime.UtcNow,
                        OverrideEndDate = expiresAt,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.FeatureFlagOverrides.Add(newOverride);
                }

                await _context.SaveChangesAsync();

                // Invalidate user cache
                await InvalidateUserCacheAsync(userId);

                _logger.LogInformation("Set feature override for user {UserId}, feature {FeatureName}, enabled: {IsEnabled}",
                    userId, featureName, isEnabled);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting user feature override for user {UserId}, feature {FeatureName}",
                    userId, featureName);
                return false;
            }
        }

        public async Task<List<FeatureFlag>> GetActiveFeatureFlagsAsync()
        {
            if (_cache.TryGetValue(CACHE_PREFIX_FEATURE_FLAGS, out List<FeatureFlag>? cachedFlags) && cachedFlags != null)
            {
                return cachedFlags;
            }

            var now = DateTime.UtcNow;
            var flags = await _context.FeatureFlags
                .Where(f => !f.IsDeleted &&
                           f.IsEnabled &&
                           (f.StartDate == null || f.StartDate <= now) &&
                           (f.EndDate == null || f.EndDate >= now))
                .OrderByDescending(f => f.Priority)
                .ToListAsync();

            _cache.Set(CACHE_PREFIX_FEATURE_FLAGS, flags, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));
            return flags;
        }

        public async Task InvalidateUserCacheAsync(string userId)
        {
            await Task.Run(() =>
            {
                // Remove user features cache
                _cache.Remove($"{CACHE_PREFIX_USER_FEATURES}{userId}");

                // Remove all usage cache entries for this user
                // Note: Since IMemoryCache doesn't provide a way to enumerate keys,
                // we'll have to rely on explicit key patterns we know about
                _cache.Remove($"{CACHE_PREFIX_USER_USAGE}{userId}");

                _logger.LogDebug("Invalidated cache for user {UserId}", userId);
            });
        }

        // Private helper methods
        private async Task<FeatureFlagOverride?> GetUserFeatureOverrideAsync(string userId, string featureName)
        {
            var now = DateTime.UtcNow;
            return await _context.FeatureFlagOverrides
                .Include(o => o.FeatureFlag)
                .FirstOrDefaultAsync(o => o.UserId == userId &&
                                         o.FeatureFlag.Name == featureName &&
                                         !o.FeatureFlag.IsDeleted &&
                                         (o.OverrideStartDate == null || o.OverrideStartDate <= now) &&
                                         (o.OverrideEndDate == null || o.OverrideEndDate >= now));
        }

        private async Task<List<FeatureFlag>> GetFeatureFlagsForFeatureAsync(string featureName)
        {
            var now = DateTime.UtcNow;
            return await _context.FeatureFlags
                .Where(f => f.Name == featureName &&
                           !f.IsDeleted &&
                           f.IsEnabled &&
                           (f.StartDate == null || f.StartDate <= now) &&
                           (f.EndDate == null || f.EndDate >= now))
                .OrderByDescending(f => f.Priority)
                .ToListAsync();
        }

        private async Task<bool?> EvaluateFeatureFlagAsync(string userId, FeatureFlag flag, Dictionary<string, object>? context)
        {
            try
            {
                // Check time-based restrictions
                var now = DateTime.UtcNow;
                if (flag.StartDate.HasValue && flag.StartDate.Value > now)
                    return false;
                if (flag.EndDate.HasValue && flag.EndDate.Value < now)
                    return false;

                // Check A/B testing rollout percentage
                if (flag.RolloutPercentage.HasValue)
                {
                    var userHash = Math.Abs(userId.GetHashCode()) % 100;
                    if (userHash >= flag.RolloutPercentage.Value)
                        return false;
                }

                // Check package requirements
                if (!string.IsNullOrEmpty(flag.RequiredPackageTypes))
                {
                    var requiredTypes = JsonConvert.DeserializeObject<List<string>>(flag.RequiredPackageTypes);
                    if (requiredTypes?.Any() == true)
                    {
                        var userSubscription = await _context.UserSubscriptions
                            .Include(s => s.Package)
                            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

                        if (userSubscription?.Package == null || !requiredTypes.Contains(userSubscription.Package.Type))
                            return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating feature flag {FlagName} for user {UserId}", flag.Name, userId);
                return null;
            }
        }

        private async Task LogFeatureAccessAsync(string userId, string featureName, bool accessGranted, string reason,
            Dictionary<string, object>? context, long evaluationTimeMs)
        {
            try
            {
                var log = new FeatureAccessLog
                {
                    UserId = userId,
                    FeatureName = featureName,
                    AccessGranted = accessGranted,
                    AccessReason = reason,
                    RequestContext = JsonConvert.SerializeObject(context ?? new Dictionary<string, object>()),
                    EvaluationTimeMs = evaluationTimeMs,
                    AccessedAt = DateTime.UtcNow
                };

                _context.FeatureAccessLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging feature access for user {UserId}, feature {FeatureName}", userId, featureName);
            }
        }

        private static DateTime GetPeriodEndDate(DateTime startDate, string period)
        {
            return period?.ToLower() switch
            {
                "daily" => startDate.AddDays(1),
                "weekly" => startDate.AddDays(7),
                "monthly" => startDate.AddMonths(1),
                "yearly" => startDate.AddYears(1),
                _ => startDate.AddMonths(1) // Default to monthly
            };
        }
    }
}
