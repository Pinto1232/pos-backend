using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public required string Email { get; set; }

        [Phone]
        [StringLength(20)]
        public required string Phone { get; set; }

        [StringLength(200)]
        public required string Address { get; set; }

        // Navigation properties
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<CustomerGroupMember> CustomerGroupMembers { get; set; } = new List<CustomerGroupMember>();
        public ICollection<CustomerFeedback> CustomerFeedbacks { get; set; } = new List<CustomerFeedback>();
        public required LoyaltyPoint LoyaltyPoint { get; set; }
    }
}
