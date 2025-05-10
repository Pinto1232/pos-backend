using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class UsageBasedPricing
    {
        [Key]
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
}
