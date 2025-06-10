using System.ComponentModel.DataAnnotations;

namespace PosBackend.Models
{
    public class PackageTier
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public int Level { get; set; }
        
        public decimal MinPrice { get; set; }
        
        public decimal MaxPrice { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [Required]
        public string EnabledFeaturesJson { get; set; } = "[]";
        
        [Required]
        public string RestrictedFeaturesJson { get; set; } = "[]";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ICollection<PricingPackage>? PricingPackages { get; set; }
    }
}