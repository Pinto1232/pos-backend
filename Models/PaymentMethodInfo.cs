using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class PaymentMethodInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string StripeCustomerId { get; set; } = string.Empty;

        [Required]
        public string StripePaymentMethodId { get; set; } = string.Empty;

        [StringLength(20)]
        public string Type { get; set; } = string.Empty; // card, bank_account, etc.

        // Card-specific information
        [StringLength(20)]
        public string? CardBrand { get; set; } // visa, mastercard, amex, etc.

        [StringLength(4)]
        public string? CardLast4 { get; set; }

        public int? CardExpMonth { get; set; }
        public int? CardExpYear { get; set; }

        [StringLength(50)]
        public string? CardCountry { get; set; }

        [StringLength(20)]
        public string? CardFunding { get; set; } // credit, debit, prepaid

        // Status and metadata
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUsedAt { get; set; }

        // Expiration tracking
        public DateTime? ExpirationDate { get; set; }
        public bool ExpirationWarning30DaysSent { get; set; } = false;
        public bool ExpirationWarning7DaysSent { get; set; } = false;
        public bool ExpirationWarning1DaySent { get; set; } = false;

        // Computed properties
        [NotMapped]
        public bool IsExpired => ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow;

        [NotMapped]
        public bool IsExpiringSoon => ExpirationDate.HasValue &&
            ExpirationDate.Value <= DateTime.UtcNow.AddDays(30) &&
            ExpirationDate.Value > DateTime.UtcNow;

        [NotMapped]
        public int DaysUntilExpiration => ExpirationDate.HasValue ?
            Math.Max(0, (int)(ExpirationDate.Value - DateTime.UtcNow).TotalDays) :
            int.MaxValue;

        public void UpdateExpirationDate()
        {
            if (CardExpMonth.HasValue && CardExpYear.HasValue)
            {
                // Stripe provides 2-digit year, convert to 4-digit
                var year = CardExpYear.Value < 100 ? 2000 + CardExpYear.Value : CardExpYear.Value;
                // Set to last day of expiration month
                ExpirationDate = new DateTime(year, CardExpMonth.Value, DateTime.DaysInMonth(year, CardExpMonth.Value), 0, 0, 0, DateTimeKind.Utc);
            }
        }
    }
}
