using System.ComponentModel.DataAnnotations;

namespace PosBackend.DTOs
{
    // Lightweight DTO for listing categories
    public class CategoryListDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public int ProductCount { get; set; }
        public int ChildCategoryCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }

    // Detailed DTO for single category view
    public class CategoryDetailDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public int ProductCount { get; set; }
        public int ChildCategoryCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public List<CategorySummaryDto> ChildCategories { get; set; } = new();
        public List<ProductSummaryDto> Products { get; set; } = new();
    }

    // Create/Update DTO
    public class CategoryCreateDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public int? ParentCategoryId { get; set; }
    }

    public class CategoryUpdateDto : CategoryCreateDto
    {
        public int CategoryId { get; set; }
    }

    // Simple DTO for dropdowns and references
    public class CategorySummaryDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public int ProductCount { get; set; }
    }

    // Hierarchical DTO for category trees
    public class CategoryTreeDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public List<CategoryTreeDto> Children { get; set; } = new();
    }
}