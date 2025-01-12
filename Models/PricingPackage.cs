// PricingPackage model
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PosBackend.Models
{
    public class PricingPackage
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; // Store as string
        public string Icon { get; set; } = string.Empty;
        public string ExtraDescription { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int TestPeriodDays { get; set; }

        [NotMapped]
        public List<string> DescriptionList
        {
            get => Description.Split(';').ToList();
            set => Description = string.Join(';', value);
        }
    }
}
