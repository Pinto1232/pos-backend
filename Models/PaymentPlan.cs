using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Models
{
    public class PaymentPlan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Period { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,4)")]
        [Range(0, 1)]
        public decimal DiscountPercentage { get; set; }

        [StringLength(20)]
        public string? DiscountLabel { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        public bool IsPopular { get; set; } = false;

        public bool IsDefault { get; set; } = false;

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        [Required]
        [StringLength(500)]
        public string ApplicableRegions { get; set; } = "*"; // JSON array as string

        [Required]
        [StringLength(500)]
        public string ApplicableUserTypes { get; set; } = "*"; // JSON array as string

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Helper methods to work with JSON arrays
        public string[] GetApplicableRegions()
        {
            if (string.IsNullOrEmpty(ApplicableRegions) || ApplicableRegions == "*")
                return new[] { "*" };
            
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<string[]>(ApplicableRegions) ?? new[] { "*" };
            }
            catch
            {
                return new[] { "*" };
            }
        }

        public void SetApplicableRegions(string[] regions)
        {
            if (regions == null || regions.Length == 0 || (regions.Length == 1 && regions[0] == "*"))
            {
                ApplicableRegions = "*";
            }
            else
            {
                ApplicableRegions = System.Text.Json.JsonSerializer.Serialize(regions);
            }
        }

        public string[] GetApplicableUserTypes()
        {
            if (string.IsNullOrEmpty(ApplicableUserTypes) || ApplicableUserTypes == "*")
                return new[] { "*" };
            
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<string[]>(ApplicableUserTypes) ?? new[] { "*" };
            }
            catch
            {
                return new[] { "*" };
            }
        }

        public void SetApplicableUserTypes(string[] userTypes)
        {
            if (userTypes == null || userTypes.Length == 0 || (userTypes.Length == 1 && userTypes[0] == "*"))
            {
                ApplicableUserTypes = "*";
            }
            else
            {
                ApplicableUserTypes = System.Text.Json.JsonSerializer.Serialize(userTypes);
            }
        }

        // Check if plan is currently valid
        public bool IsCurrentlyValid()
        {
            var now = DateTime.UtcNow;
            return IsActive && 
                   (ValidFrom == null || ValidFrom <= now) && 
                   (ValidTo == null || ValidTo >= now);
        }

        // Check if plan applies to specific region
        public bool AppliesToRegion(string? region)
        {
            if (string.IsNullOrEmpty(region)) return true;
            
            var regions = GetApplicableRegions();
            return regions.Contains("*") || regions.Contains(region, StringComparer.OrdinalIgnoreCase);
        }

        // Check if plan applies to specific user type
        public bool AppliesToUserType(string? userType)
        {
            if (string.IsNullOrEmpty(userType)) return true;
            
            var userTypes = GetApplicableUserTypes();
            return userTypes.Contains("*") || userTypes.Contains(userType, StringComparer.OrdinalIgnoreCase);
        }
    }
}
