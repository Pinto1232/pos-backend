using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class PaymentRetryAttempt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string StripeSubscriptionId { get; set; } = string.Empty;

        [Required]
        public string StripeInvoiceId { get; set; } = string.Empty;

        [Required]
        public string StripePaymentIntentId { get; set; } = string.Empty;

        public int AttemptNumber { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty; // pending, succeeded, failed, cancelled

        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public DateTime? NextRetryAt { get; set; }

        // Failure information
        public string? FailureCode { get; set; }
        public string? FailureMessage { get; set; }
        public string? DeclineCode { get; set; }

        // Retry strategy information
        [StringLength(50)]
        public string RetryStrategy { get; set; } = "exponential_backoff"; // exponential_backoff, fixed_interval, smart_retry

        public int RetryIntervalHours { get; set; } = 1;
        public bool IsAutomaticRetry { get; set; } = true;
        public bool IsManualRetry { get; set; } = false;

        // Notification tracking
        public bool NotificationSent { get; set; } = false;
        public DateTime? NotificationSentAt { get; set; }

        // Metadata
        public string? StripeEventId { get; set; }
        public string? AdditionalMetadata { get; set; } // JSON for extra data

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        // Note: StripeSubscriptionId is a string identifier, not a foreign key to the StripeSubscription table

        // Computed properties
        [NotMapped]
        public bool IsRetryDue => NextRetryAt.HasValue && NextRetryAt.Value <= DateTime.UtcNow && Status == "failed";

        [NotMapped]
        public bool IsMaxRetriesReached => AttemptNumber >= 4; // Configurable max retries

        [NotMapped]
        public TimeSpan? TimeUntilNextRetry => NextRetryAt.HasValue ? NextRetryAt.Value - DateTime.UtcNow : null;

        public void CalculateNextRetryTime(int[] retryIntervalHours)
        {
            if (AttemptNumber < retryIntervalHours.Length)
            {
                var intervalHours = retryIntervalHours[AttemptNumber - 1];
                NextRetryAt = DateTime.UtcNow.AddHours(intervalHours);
                RetryIntervalHours = intervalHours;
            }
            else
            {
                // Max retries reached, no more retries
                NextRetryAt = null;
            }
        }

        public void MarkAsSucceeded()
        {
            Status = "succeeded";
            CompletedAt = DateTime.UtcNow;
            NextRetryAt = null;
            LastUpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string failureCode, string failureMessage, string? declineCode = null)
        {
            Status = "failed";
            FailureCode = failureCode;
            FailureMessage = failureMessage;
            DeclineCode = declineCode;
            CompletedAt = DateTime.UtcNow;
            LastUpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsCancelled()
        {
            Status = "cancelled";
            CompletedAt = DateTime.UtcNow;
            NextRetryAt = null;
            LastUpdatedAt = DateTime.UtcNow;
        }
    }
}
