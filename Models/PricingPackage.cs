using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PosBackend.Models
{
    public class PricingPackage
    {
        [Key]
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

        public bool IsCustomizable => Type.ToLower() == "custom";

        // Navigation properties
        public ICollection<CustomPackageSelectedFeature>? SelectedFeatures { get; set; }
        public ICollection<CustomPackageSelectedAddOn>? SelectedAddOns { get; set; }
        public ICollection<CustomPackageUsageBasedPricing>? SelectedUsageBasedPricing { get; set; }
    }
}
