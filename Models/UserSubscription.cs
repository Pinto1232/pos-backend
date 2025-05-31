using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PosBackend.Models
{
    public class UserSubscription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int PricingPackageId { get; set; }

        [ForeignKey("PricingPackageId")]
        public PricingPackage? Package { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Stripe subscription fields
        public string? StripeSubscriptionId { get; set; }
        public string? StripeCustomerId { get; set; }
        public string? StripePriceId { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "active"; // active, trialing, past_due, canceled, unpaid

        public DateTime? TrialStart { get; set; }
        public DateTime? TrialEnd { get; set; }
        public DateTime? CurrentPeriodStart { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }

        public bool CancelAtPeriodEnd { get; set; } = false;
        public DateTime? CanceledAt { get; set; }

        // Payment and billing
        public decimal? LastPaymentAmount { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public DateTime? NextBillingDate { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        public string EnabledFeaturesJson { get; set; } = "[]";

        [NotMapped]
        public List<string> EnabledFeatures
        {
            get => string.IsNullOrEmpty(EnabledFeaturesJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(EnabledFeaturesJson) ?? new List<string>();
            set => EnabledFeaturesJson = JsonSerializer.Serialize(value ?? new List<string>());
        }

        // Additional packages that the user has enabled beyond their main subscription
        public string AdditionalPackagesJson { get; set; } = "[]";

        [NotMapped]
        public List<int> AdditionalPackages
        {
            get => string.IsNullOrEmpty(AdditionalPackagesJson)
                ? new List<int>()
                : JsonSerializer.Deserialize<List<int>>(AdditionalPackagesJson) ?? new List<int>();
            set => AdditionalPackagesJson = JsonSerializer.Serialize(value ?? new List<int>());
        }

        // Audit tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
