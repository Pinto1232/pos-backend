using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class CustomPackageSelectedAddOn
    {
        public int Id { get; set; }
        public int PricingPackageId { get; set; }
        public int AddOnId { get; set; }
        public AddOn? AddOn { get; set; }
    }
}
