using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class PackagePrice
    {
        public int Id { get; set; }
        
        [Required]
        public int PackageId { get; set; }
        
        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ValidUntil { get; set; }
        
        [ForeignKey("PackageId")]
        public PricingPackage Package { get; set; } = null!;
    }
    
    public class PackagePriceDto
    {
        public int PackageId { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ValidUntil { get; set; }
    }
}