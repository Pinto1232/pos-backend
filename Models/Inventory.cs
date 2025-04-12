using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class Inventory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InventoryId { get; set; }

        [ForeignKey("Store")]
        public int StoreId { get; set; }
        public Store? Store { get; set; }

        [ForeignKey("ProductVariant")]
        public int VariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }

        public int Quantity { get; set; }

        public int ReorderLevel { get; set; }

        public DateTime LastUpdated { get; set; }

        // Navigation properties
        public ICollection<StockAlert>? StockAlerts { get; set; }
    }
}
