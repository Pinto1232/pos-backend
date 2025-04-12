using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class Invoice : IEquatable<Invoice>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InvoiceId { get; set; }

        [Required(ErrorMessage = "Sale is required")]
        [ForeignKey("Sale")]
        public int SaleId { get; set; }
        public Sale? Sale { get; set; }

        [Required(ErrorMessage = "Invoice Number is required")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Invoice Number must be between 5 and 50 characters")]
        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime IssuedDate { get; set; }

        public DateTime DueDate { get; set; }

        // Audit and Soft Delete Tracking
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Invoice Status Properties
        public bool IsPastDue => DateTime.Now > DueDate;
        public bool IsPaid { get; set; } = false;
        public decimal TotalAmount { get; set; }

        // Invoice Management Methods
        public void MarkAsPaid()
        {
            IsPaid = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public TimeSpan GetRemainingTime()
        {
            return DueDate - DateTime.Now;
        }

        public bool Equals(Invoice? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return InvoiceId == other.InvoiceId &&
                   InvoiceNumber == other.InvoiceNumber &&
                   SaleId == other.SaleId;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Invoice)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(InvoiceId, InvoiceNumber, SaleId);
        }
    }
}
