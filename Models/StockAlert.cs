using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class StockAlert
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AlertId { get; set; }

        [ForeignKey("Inventory")]
        public int InventoryId { get; set; }
        public required Inventory Inventory { get; set; }

        [StringLength(50)]
        public required string AlertType { get; set; }

        public int Threshold { get; set; }

        public bool IsActive { get; set; }
    }
}
