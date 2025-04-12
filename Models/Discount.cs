using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class Discount : IEquatable<Discount>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DiscountId { get; set; }

        [Required(ErrorMessage = "Discount Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Discount Type is required")]
        [StringLength(50, ErrorMessage = "Discount Type cannot exceed 50 characters")]
        public string DiscountType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100, ErrorMessage = "Discount value must be between 0 and 100 for percentage discounts")]
        public decimal Value { get; set; }

        public DateTime? ActiveUntil { get; set; }

        // Audit and Soft Delete Tracking
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        // Computed Properties for Discount Status
        public bool IsActive => !ActiveUntil.HasValue || ActiveUntil.Value >= DateTime.UtcNow;
        public bool HasCoupons => Coupons?.Any() == true;

        // Navigation property
        public ICollection<Coupon>? Coupons { get; set; }

        // Discount Calculation Method
        public decimal CalculateDiscountedAmount(decimal originalPrice)
        {
            if (!IsActive)
                return originalPrice;

            return DiscountType?.ToLower() switch
            {
                "percentage" => originalPrice * (1 - (Value / 100)),
                "fixed" => Math.Max(originalPrice - Value, 0),
                _ => originalPrice
            };
        }

        public bool Equals(Discount? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return DiscountId == other.DiscountId &&
                   Name == other.Name &&
                   DiscountType == other.DiscountType;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Discount)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DiscountId, Name, DiscountType);
        }
    }
}
