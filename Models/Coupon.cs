using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class Coupon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CouponId { get; set; }

        [Required]
        [StringLength(50)]
        public required string Code { get; set; }

        [ForeignKey("Discount")]
        public int DiscountId { get; set; }
        public required Discount Discount { get; set; }

        public int? UsageLimit { get; set; }

        public int TimesUsed { get; set; }
    }
}
