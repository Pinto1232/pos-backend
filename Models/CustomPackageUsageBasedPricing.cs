using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class CustomPackageUsageBasedPricing
    {
        public int Id { get; set; }
        public int PricingPackageId { get; set; }
        public int UsageBasedPricingId { get; set; }
        public int Quantity { get; set; }
        public UsageBasedPricing? UsageBasedPricing { get; set; }
    }
}
