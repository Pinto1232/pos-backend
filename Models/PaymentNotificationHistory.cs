using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class PaymentNotificationHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string NotificationType { get; set; } = string.Empty; // CardExpiration, UpcomingPayment, PaymentFailed, PaymentRetrySuccess, etc.

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DeliveryMethod { get; set; } = string.Empty; // Email, SMS, Push, InApp

        [Required]
        [StringLength(200)]
        public string Recipient { get; set; } = string.Empty; // email address, phone number, etc.

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty; // Sent, Failed, Pending, Delivered

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }

        // Related entities
        public string? StripeSubscriptionId { get; set; }
        public string? StripePaymentMethodId { get; set; }
        public string? StripeInvoiceId { get; set; }

        // Metadata
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; } = 0;
        public DateTime? NextRetryAt { get; set; }

        // Additional context data (JSON)
        public string? ContextData { get; set; }

        // Tracking fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
