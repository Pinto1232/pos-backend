using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PosBackend.Models
{
    public class PricingPackage
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string ExtraDescription { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int TestPeriodDays { get; set; }
        public string Type { get; set; } = string.Empty;

        // default/base currency (e.g., "USD").
        public string Currency { get; set; } = "";

        // JSON string to store prices for multiple currencies.
        public string MultiCurrencyPrices { get; set; } = "{}";

        [NotMapped]
        public List<string> DescriptionList
        {
            get => Description.Split(';').ToList();
            set => Description = string.Join(';', value);
        }

        public bool IsCustomizable => Type.ToLower() == "custom" || Type.ToLower() == "custom-pro";

        // Navigation properties
        public ICollection<CustomPackageSelectedFeature>? SelectedFeatures { get; set; }
        public ICollection<CustomPackageSelectedAddOn>? SelectedAddOns { get; set; }
        public ICollection<CustomPackageUsageBasedPricing>? SelectedUsageBasedPricing { get; set; }
    }

    public class Feature
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public bool IsRequired { get; set; }
    }

    public class AddOn
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }

        // Default/base currency (e.g., "USD")
        public string Currency { get; set; } = "USD";

        // JSON string to store prices for multiple currencies
        public string MultiCurrencyPrices { get; set; } = "{}";

        // Category for grouping related add-ons
        public string Category { get; set; } = string.Empty;

        // Flag to indicate if the add-on is active/available
        public bool IsActive { get; set; } = true;

        // JSON string to store specific capabilities/functionalities that the addon enables
        public string Features { get; set; } = "[]";

        // JSON string to store any requirements or prerequisites needed for the addon to function
        public string Dependencies { get; set; } = "[]";

        // Icon or visual indicator for the addon (can be a URL or a class name)
        public string Icon { get; set; } = string.Empty;
    }

    public class UsageBasedPricing
    {
        public int Id { get; set; }
        public int FeatureId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public decimal PricePerUnit { get; set; }

        [NotMapped]
        public int DefaultValue => MinValue;
    }

    // Junction tables
    public class CustomPackageSelectedFeature
    {
        public int Id { get; set; }
        public int PricingPackageId { get; set; }
        public int FeatureId { get; set; }
        public Feature? Feature { get; set; }
    }

    public class CustomPackageSelectedAddOn
    {
        public int Id { get; set; }
        public int PricingPackageId { get; set; }
        public int AddOnId { get; set; }
        public AddOn? AddOn { get; set; }
    }

    public class CustomPackageUsageBasedPricing
    {
        public int Id { get; set; }
        public int PricingPackageId { get; set; }
        public int UsageBasedPricingId { get; set; }
        public int Quantity { get; set; }
        public UsageBasedPricing? UsageBasedPricing { get; set; }
    }
}
