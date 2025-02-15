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

        [NotMapped]
        public List<Feature> CoreFeatures { get; set; } = new();

        [NotMapped]
        public List<AddOn> AddOns { get; set; } = new();

        [NotMapped]
        public List<UsageBasedPricing> UsageBasedPricingOptions { get; set; } = new();
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

        [NotMapped]
        public List<int> Dependencies { get; set; } = new();
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

}
