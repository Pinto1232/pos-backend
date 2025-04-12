using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentId { get; set; }

        [ForeignKey("Sale")]
        public int SaleId { get; set; }
        public required Sale Sale { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(50)]
        public required string PaymentMethod { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
