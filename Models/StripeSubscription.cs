using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class StripeSubscription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string StripeSubscriptionId { get; set; } = string.Empty;

        [Required]
        public string StripeCustomerId { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int UserSubscriptionId { get; set; }

        [ForeignKey("UserSubscriptionId")]
        public UserSubscription? UserSubscription { get; set; }

        [Required]
        public string StripePriceId { get; set; } = string.Empty;

        [Required]
        public string StripeProductId { get; set; } = string.Empty;

        [StringLength(50)]
        public string Status { get; set; } = string.Empty; // active, trialing, past_due, canceled, unpaid, incomplete, incomplete_expired

        public DateTime? TrialStart { get; set; }
        public DateTime? TrialEnd { get; set; }
        public DateTime? CurrentPeriodStart { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }

        public bool CancelAtPeriodEnd { get; set; } = false;
        public DateTime? CanceledAt { get; set; }
        public DateTime? EndedAt { get; set; }

        // Latest invoice information
        public string? LatestInvoiceId { get; set; }
        public decimal? LatestInvoiceAmount { get; set; }
        public string? LatestInvoiceStatus { get; set; }
        public DateTime? LatestInvoiceDate { get; set; }

        // Payment method information
        public string? DefaultPaymentMethodId { get; set; }
        public string? PaymentMethodType { get; set; } // card, bank_account, etc.
        public string? PaymentMethodLast4 { get; set; }
        public string? PaymentMethodBrand { get; set; }

        // Billing and pricing
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        public string BillingInterval { get; set; } = "month"; // month, year
        public int BillingIntervalCount { get; set; } = 1;

        // Metadata for additional information
        public string? Metadata { get; set; } = "{}";

        // Audit tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastSyncedAt { get; set; }

        // Webhook tracking
        public string? LastWebhookEventId { get; set; }
        public DateTime? LastWebhookEventDate { get; set; }

        // Retry and failure tracking
        public int FailedPaymentAttempts { get; set; } = 0;
        public DateTime? LastFailedPaymentDate { get; set; }
        public string? LastFailureReason { get; set; }

        // Discount and coupon information
        public string? CouponId { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public DateTime? DiscountStart { get; set; }
        public DateTime? DiscountEnd { get; set; }
    }
}
