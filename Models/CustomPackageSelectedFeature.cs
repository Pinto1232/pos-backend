using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class CustomPackageSelectedFeature
    {
        public int Id { get; set; }
        public int PricingPackageId { get; set; }
        public int FeatureId { get; set; }
        public Feature? Feature { get; set; }
    }
}
