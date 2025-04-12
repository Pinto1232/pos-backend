using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class CustomerFeedback
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FeedbackId { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public required Customer Customer { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public required Product Product { get; set; }

        public int Rating { get; set; }
    }
}
