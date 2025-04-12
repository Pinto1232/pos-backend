using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class Product : IEquatable<Product>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required(ErrorMessage = "Supplier is required")]
        [ForeignKey("Supplier")]
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }

        // Audit and Soft Delete Tracking
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        // Computed Properties for Product Insights
        public bool HasVariants => ProductVariants?.Any() == true;
        public int VariantCount => ProductVariants?.Count ?? 0;
        public decimal? AverageVariantPrice => ProductVariants?.Any() == true
            ? ProductVariants.Average(v => v.Price)
            : null;

        // Navigation properties
        public ICollection<ProductVariant>? ProductVariants { get; set; }
        public ICollection<ProductExpiry>? ProductExpiries { get; set; }
        public ICollection<CustomerFeedback>? CustomerFeedbacks { get; set; }

        // Price Calculation Method
        public decimal CalculateDiscountedPrice(decimal discountPercentage)
        {
            if (discountPercentage < 0 || discountPercentage > 100)
                throw new ArgumentException("Discount percentage must be between 0 and 100");

            return BasePrice * (1 - (discountPercentage / 100));
        }

        // Implement IEquatable<T> for robust comparison
        public bool Equals(Product? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ProductId == other.ProductId &&
                   Name == other.Name &&
                   CategoryId == other.CategoryId;
        }

        // Override Equals and GetHashCode for comprehensive comparison
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Product)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ProductId, Name, CategoryId);
        }
    }
}
