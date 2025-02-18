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

        [NotMapped]
        public List<string> DescriptionList
        {
            get => Description.Split(';').ToList();
            set => Description = string.Join(';', value);
        }

        public bool IsCustomizable => Type.ToLower() == "custom";

        // Only applies to the "Custom" package
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
    }

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
