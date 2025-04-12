using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class ProductVariant
    {
        public ProductVariant()
        {
            // Initialize collections 
            Inventories = new List<Inventory>();
            InventoryMovements = new List<InventoryMovement>();
            ProductExpiries = new List<ProductExpiry>();
            SaleItems = new List<SaleItem>();
            OrderItems = new List<OrderItem>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VariantId { get; set; }
        public decimal Price { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!; 

        [StringLength(50)]
        public required string Sku { get; set; } 

        public required string Attributes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceAdjustment { get; set; }

        [StringLength(50)]
        public required string Barcode { get; set; }

        // Navigation properties
        public ICollection<Inventory> Inventories { get; set; }
        public ICollection<InventoryMovement> InventoryMovements { get; set; }
        public ICollection<ProductExpiry> ProductExpiries { get; set; }
        public ICollection<SaleItem> SaleItems { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}