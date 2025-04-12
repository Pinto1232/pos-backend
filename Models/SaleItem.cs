using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class SaleItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SaleItemId { get; set; }

        [ForeignKey("Sale")]
        public int SaleId { get; set; }
        public required Sale Sale { get; set; }

        [ForeignKey("ProductVariant")]
        public int VariantId { get; set; }
        public required ProductVariant ProductVariant { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }
    }
}
