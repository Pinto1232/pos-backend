using System.ComponentModel.DataAnnotations;
using PosBackend.Attributes;
using PosBackend.Services;

namespace PosBackend.Models.DTOs
{
    public class PricingPackageDto
    {
        public int Id { get; set; }
        
        [SanitizeString(InputType.PlainText, 100)]
        public string Title { get; set; } = string.Empty;
        
        [SanitizeString(InputType.PlainText, 500)]
        public string Description { get; set; } = string.Empty;
        
        [SanitizeString(InputType.PlainText, 50)]
        public string Icon { get; set; } = string.Empty;
        
        [SanitizeString(InputType.PlainText, 1000)]
        public string ExtraDescription { get; set; } = string.Empty;
        
        public decimal Price { get; set; }
        public int TestPeriodDays { get; set; }
        
        [SanitizeString(InputType.Alphanumeric, 50)]
        public string Type { get; set; } = string.Empty;
        
        public List<string> DescriptionList { get; set; } = new List<string>();
        public bool IsCustomizable { get; set; }
        
        [SanitizeString(InputType.Alphanumeric, 10)]
        public string Currency { get; set; } = string.Empty;
        
        [ValidJson]
        public string MultiCurrencyPrices { get; set; } = string.Empty;
        
        // Tier information
        public int? TierId { get; set; }
        public int TierLevel { get; set; }
        public string? TierName { get; set; }
        public string? TierDescription { get; set; }
    }

    public class CreatePricingPackageDto
    {
        [Required]
        [SanitizeString(InputType.PlainText, 100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [SanitizeString(InputType.PlainText, 500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [SanitizeString(InputType.PlainText, 50)]
        public string Icon { get; set; } = string.Empty;

        [SanitizeString(InputType.PlainText, 1000)]
        public string ExtraDescription { get; set; } = string.Empty;

        [Required]
        [Range(0, 999999.99)]
        public decimal Price { get; set; }

        [Range(0, 365)]
        public int TestPeriodDays { get; set; } = 14;

        [Required]
        [SanitizeString(InputType.Alphanumeric, 50)]
        public string Type { get; set; } = string.Empty;

        [SanitizeString(InputType.Alphanumeric, 10)]
        public string Currency { get; set; } = "USD";

        [ValidJson]
        public string MultiCurrencyPrices { get; set; } = "{}";
    }

    public class CustomSelectionRequest
    {
        public int PackageId { get; set; }
        public List<int> SelectedFeatures { get; set; } = new List<int>();
        public List<int> SelectedAddOns { get; set; } = new List<int>();
        public Dictionary<int, int> UsageLimits { get; set; } = new Dictionary<int, int>();
    }

    public class CustomPricingRequest
    {
        public int PackageId { get; set; }
        public List<int> SelectedFeatures { get; set; } = new List<int>();
        public List<int> SelectedAddOns { get; set; } = new List<int>();
        public Dictionary<int, int> UsageLimits { get; set; } = new Dictionary<int, int>();
    }
}