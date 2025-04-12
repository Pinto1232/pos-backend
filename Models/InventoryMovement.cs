using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class InventoryMovement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MovementId { get; set; }

        [ForeignKey("Inventory")]
        public int InventoryId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be positive")]
        public int Quantity { get; set; }

        public Inventory Inventory { get; set; } = null!; 

     
        [ForeignKey("Store")]
        public int StoreId { get; set; }
        public Store Store { get; set; } = null!;

     
        [ForeignKey("ProductVariant")]
        public int VariantId { get; set; }
        public ProductVariant ProductVariant { get; set; } = null!;

        public int QuantityChange { get; set; }

        public required string Type { get; set; }

        public DateTime MovementDate { get; set; }

        [ForeignKey("User")]
        public int? UserId { get; set; }
        public User? User { get; set; }

        public string? Reason { get; set; }
    }
}
