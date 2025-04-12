using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class CustomerGroupMember : IEquatable<CustomerGroupMember>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MembershipId { get; set; }

        [Required(ErrorMessage = "Customer is required")]
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Required(ErrorMessage = "Customer Group is required")]
        [ForeignKey("CustomerGroup")]
        public int GroupId { get; set; }
        public CustomerGroup? CustomerGroup { get; set; }

        // Membership Tracking Properties
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public DateTime? InactivatedAt { get; set; }

        // Computed Properties for Membership Insights
        public bool IsRecentMember => JoinedAt > DateTime.UtcNow.AddMonths(-3);
        public TimeSpan MembershipDuration => DateTime.UtcNow - JoinedAt;

        // Membership Management Methods
        public void Inactivate()
        {
            if (IsActive)
            {
                IsActive = false;
                InactivatedAt = DateTime.UtcNow;
            }
        }

        public void Reactivate()
        {
            if (!IsActive)
            {
                IsActive = true;
                InactivatedAt = null;
            }
        }

        public bool Equals(CustomerGroupMember? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return MembershipId == other.MembershipId &&
                   CustomerId == other.CustomerId &&
                   GroupId == other.GroupId;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CustomerGroupMember)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MembershipId, CustomerId, GroupId);
        }
    }
}
