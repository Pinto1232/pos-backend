using Microsoft.AspNetCore.Mvc;
using PosBackend.Models;
using PosBackend.Services;

namespace PosBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeatureFlagController : ControllerBase
    {
        private readonly IAdvancedPermissionService _permissionService;
        private readonly ILogger<FeatureFlagController> _logger;

        public FeatureFlagController(
            IAdvancedPermissionService permissionService,
            ILogger<FeatureFlagController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        // GET: api/FeatureFlag/user/{userId}/access/{featureName}
        [HttpGet("user/{userId}/access/{featureName}")]
        public async Task<ActionResult<FeatureAccessResult>> CheckFeatureAccess(string userId, string featureName)
        {
            try
            {
                var context = new Dictionary<string, object>
                {
                    ["userAgent"] = Request.Headers.UserAgent.ToString(),
                    ["ipAddress"] = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    ["timestamp"] = DateTime.UtcNow
                };

                var result = await _permissionService.EvaluateFeatureAccessAsync(userId, featureName, context);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking feature access for user {UserId}, feature {FeatureName}", userId, featureName);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // GET: api/FeatureFlag/user/{userId}/features
        [HttpGet("user/{userId}/features")]
        public async Task<ActionResult<List<string>>> GetUserFeatures(string userId)
        {
            try
            {
                var features = await _permissionService.GetUserAvailableFeaturesAsync(userId);
                return Ok(features);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user features for user {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // POST: api/FeatureFlag/user/{userId}/track-usage/{featureName}
        [HttpPost("user/{userId}/track-usage/{featureName}")]
        public async Task<ActionResult> TrackFeatureUsage(string userId, string featureName, [FromBody] Dictionary<string, object>? metadata = null)
        {
            try
            {
                var success = await _permissionService.TrackFeatureUsageAsync(userId, featureName, metadata);
                if (success)
                {
                    return Ok(new { message = "Usage tracked successfully" });
                }
                return BadRequest(new { error = "Failed to track usage" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking feature usage for user {UserId}, feature {FeatureName}", userId, featureName);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // GET: api/FeatureFlag/user/{userId}/usage/{featureName}
        [HttpGet("user/{userId}/usage/{featureName}")]
        public async Task<ActionResult<FeatureUsageInfo>> GetFeatureUsage(string userId, string featureName)
        {
            try
            {
                var usageInfo = await _permissionService.GetFeatureUsageInfoAsync(userId, featureName);
                return Ok(usageInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feature usage for user {UserId}, feature {FeatureName}", userId, featureName);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // POST: api/FeatureFlag
        [HttpPost]
        public async Task<ActionResult> CreateFeatureFlag([FromBody] CreateFeatureFlagRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Description))
                {
                    return BadRequest(new { error = "Name and Description are required" });
                }

                var featureFlag = new FeatureFlag
                {
                    Name = request.Name,
                    Description = request.Description,
                    IsEnabled = request.IsEnabled,
                    Type = request.Type ?? "global",
                    Configuration = request.Configuration ?? "{}",
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    UsageLimit = request.UsageLimit,
                    UsagePeriod = request.UsagePeriod,
                    RolloutPercentage = request.RolloutPercentage,
                    TargetAudience = request.TargetAudience,
                    RequiredPackageTypes = request.RequiredPackageTypes,
                    MinimumPackageLevel = request.MinimumPackageLevel,
                    Priority = request.Priority,
                    CreatedBy = request.CreatedBy ?? "system"
                };

                var success = await _permissionService.CreateFeatureFlagAsync(featureFlag);
                if (success)
                {
                    return Ok(new { message = "Feature flag created successfully", id = featureFlag.Id });
                }
                return BadRequest(new { error = "Failed to create feature flag" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating feature flag {FeatureName}", request.Name);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // PUT: api/FeatureFlag/{flagId}
        [HttpPut("{flagId}")]
        public async Task<ActionResult> UpdateFeatureFlag(int flagId, [FromBody] UpdateFeatureFlagRequest request)
        {
            try
            {
                var featureFlag = new FeatureFlag
                {
                    Name = request.Name,
                    Description = request.Description,
                    IsEnabled = request.IsEnabled,
                    Type = request.Type,
                    Configuration = request.Configuration ?? "{}",
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    UsageLimit = request.UsageLimit,
                    UsagePeriod = request.UsagePeriod,
                    RolloutPercentage = request.RolloutPercentage,
                    TargetAudience = request.TargetAudience,
                    RequiredPackageTypes = request.RequiredPackageTypes,
                    MinimumPackageLevel = request.MinimumPackageLevel,
                    Priority = request.Priority,
                    UpdatedBy = request.UpdatedBy ?? "system"
                };

                var success = await _permissionService.UpdateFeatureFlagAsync(flagId, featureFlag);
                if (success)
                {
                    return Ok(new { message = "Feature flag updated successfully" });
                }
                return NotFound(new { error = "Feature flag not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating feature flag {FlagId}", flagId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // POST: api/FeatureFlag/user/{userId}/override/{featureName}
        [HttpPost("user/{userId}/override/{featureName}")]
        public async Task<ActionResult> SetUserOverride(string userId, string featureName, [FromBody] SetUserOverrideRequest request)
        {
            try
            {
                var success = await _permissionService.SetUserFeatureOverrideAsync(
                    userId, 
                    featureName, 
                    request.IsEnabled, 
                    request.Reason, 
                    request.ExpiresAt);

                if (success)
                {
                    return Ok(new { message = "User override set successfully" });
                }
                return BadRequest(new { error = "Failed to set user override" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting user override for user {UserId}, feature {FeatureName}", userId, featureName);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // GET: api/FeatureFlag/active
        [HttpGet("active")]
        public async Task<ActionResult<List<FeatureFlag>>> GetActiveFeatureFlags()
        {
            try
            {
                var flags = await _permissionService.GetActiveFeatureFlagsAsync();
                return Ok(flags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active feature flags");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // DELETE: api/FeatureFlag/user/{userId}/cache
        [HttpDelete("user/{userId}/cache")]
        public async Task<ActionResult> InvalidateUserCache(string userId)
        {
            try
            {
                await _permissionService.InvalidateUserCacheAsync(userId);
                return Ok(new { message = "User cache invalidated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating user cache for user {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    // Request DTOs
    public class CreateFeatureFlagRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = false;
        public string? Type { get; set; }
        public string? Configuration { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? UsageLimit { get; set; }
        public string? UsagePeriod { get; set; }
        public double? RolloutPercentage { get; set; }
        public string? TargetAudience { get; set; }
        public string? RequiredPackageTypes { get; set; }
        public int? MinimumPackageLevel { get; set; }
        public int Priority { get; set; } = 0;
        public string? CreatedBy { get; set; }
    }

    public class UpdateFeatureFlagRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Configuration { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? UsageLimit { get; set; }
        public string? UsagePeriod { get; set; }
        public double? RolloutPercentage { get; set; }
        public string? TargetAudience { get; set; }
        public string? RequiredPackageTypes { get; set; }
        public int? MinimumPackageLevel { get; set; }
        public int Priority { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class SetUserOverrideRequest
    {
        public bool IsEnabled { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }
}
