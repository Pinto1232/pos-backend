using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class ProductExpiry : IEquatable<ProductExpiry>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExpiryId { get; set; }

        [ForeignKey("ProductVariant")]
        public int VariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required(ErrorMessage = "Batch Number is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Batch Number must be between 3 and 50 characters")]
        public string BatchNumber { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ExpiryDate { get; set; }

        // Soft delete mechanism
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Computed property to check expiration status
        public bool IsExpired => ExpiryDate < DateTime.Now;

        // Implement IEquatable<T> for robust comparison
        public bool Equals(ProductExpiry? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ExpiryId == other.ExpiryId &&
                   VariantId == other.VariantId &&
                   ProductId == other.ProductId;
        }

        // Override Equals and GetHashCode for comprehensive
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProductExpiry)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ExpiryId, VariantId, ProductId);
        }
    }
}
