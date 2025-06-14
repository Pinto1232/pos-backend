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
        [StringLength(50)]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public required string LastName { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public required string Email { get; set; }

        [Phone]
        [StringLength(20)]
        public required string Phone { get; set; }

        [StringLength(200)]
        public required string Address { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastVisit { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<CustomerGroupMember> CustomerGroupMembers { get; set; } = new List<CustomerGroupMember>();
        public ICollection<CustomerFeedback> CustomerFeedbacks { get; set; } = new List<CustomerFeedback>();
        public LoyaltyPoint? LoyaltyPoint { get; set; }
    }
}
