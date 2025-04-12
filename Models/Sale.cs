using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class Sale
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SaleId { get; set; }

        [ForeignKey("Store")]
        public int StoreId { get; set; }
        public required Store Store { get; set; }

        [ForeignKey("Terminal")]
        public int TerminalId { get; set; }
        public required Terminal Terminal { get; set; }

        [ForeignKey("Customer")]
        public int? CustomerId { get; set; }
        public required Customer Customer { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public required User User { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime SaleDate { get; set; }

        // Navigation properties
        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public required Invoice Invoice { get; set; }
    }
}
