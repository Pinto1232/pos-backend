using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class FeatureFlag
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public bool IsEnabled { get; set; } = false;

        // Feature flag type: global, user-specific, package-specific, a/b-test
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = "global";

        // JSON configuration for the feature flag
        public string Configuration { get; set; } = "{}";

        // Time-based limitations
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Usage-based limitations
        public int? UsageLimit { get; set; }
        public string? UsagePeriod { get; set; } // daily, weekly, monthly, yearly

        // A/B Testing support
        public double? RolloutPercentage { get; set; } // 0.0 to 100.0
        public string? TargetAudience { get; set; } // JSON array of user segments

        // Package/subscription requirements
        public string? RequiredPackageTypes { get; set; } // JSON array of package types
        public int? MinimumPackageLevel { get; set; }

        // Priority for feature flag evaluation (higher number = higher priority)
        public int Priority { get; set; } = 0;

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;

        // Soft delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual ICollection<UserFeatureUsage> UserFeatureUsages { get; set; } = new List<UserFeatureUsage>();
        public virtual ICollection<FeatureFlagOverride> FeatureFlagOverrides { get; set; } = new List<FeatureFlagOverride>();
    }

    public class UserFeatureUsage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int FeatureFlagId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FeatureName { get; set; } = string.Empty;

        public int UsageCount { get; set; } = 0;
        public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;
        public DateTime PeriodStartDate { get; set; } = DateTime.UtcNow;
        public DateTime PeriodEndDate { get; set; }

        // Additional usage metadata (JSON)
        public string UsageMetadata { get; set; } = "{}";

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("FeatureFlagId")]
        public virtual FeatureFlag FeatureFlag { get; set; } = null!;
    }

    public class FeatureFlagOverride
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int FeatureFlagId { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public bool IsEnabled { get; set; }

        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        // Time-based override
        public DateTime? OverrideStartDate { get; set; }
        public DateTime? OverrideEndDate { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("FeatureFlagId")]
        public virtual FeatureFlag FeatureFlag { get; set; } = null!;
    }

    public class FeatureAccessLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FeatureName { get; set; } = string.Empty;

        [Required]
        public bool AccessGranted { get; set; }

        [MaxLength(500)]
        public string AccessReason { get; set; } = string.Empty;

        // Request context
        public string RequestContext { get; set; } = "{}"; // JSON with IP, user agent, etc.

        // Performance tracking
        public long EvaluationTimeMs { get; set; }

        public DateTime AccessedAt { get; set; } = DateTime.UtcNow;
    }
}
