using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [ForeignKey("ParentCategory")]
        public int? ParentCategoryId { get; set; }
        public required Category ParentCategory { get; set; }

        // Navigation properties
        public ICollection<Category> ChildCategories { get; set; } = new List<Category>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
