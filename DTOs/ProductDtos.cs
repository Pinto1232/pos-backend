using System.ComponentModel.DataAnnotations;

namespace PosBackend.DTOs
{
    // Lightweight DTO for listing products
    public class ProductListDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public int VariantCount { get; set; }
        public bool HasVariants { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }

    // Detailed DTO for single product view
    public class ProductDetailDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public bool HasVariants { get; set; }
        public int VariantCount { get; set; }
        public decimal? AverageVariantPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public List<ProductVariantDto> ProductVariants { get; set; } = new();
    }

    // Create/Update DTO
    public class ProductCreateDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Base price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Base price must be greater than 0")]
        public decimal BasePrice { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Supplier is required")]
        public int SupplierId { get; set; }
    }

    public class ProductUpdateDto : ProductCreateDto
    {
        public int ProductId { get; set; }
    }

    // Simple DTO for dropdowns and references
    public class ProductSummaryDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
    }

    public class ProductVariantDto
    {
        public int VariantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? SKU { get; set; }
        public int StockQuantity { get; set; }
    }
}