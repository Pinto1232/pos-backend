using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class CustomerGroup : IEquatable<CustomerGroup>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GroupId { get; set; }

        [Required(ErrorMessage = "Group Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        // Audit and Soft Delete Tracking
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        // Computed Properties for Group Insights
        public bool HasMembers => CustomerGroupMembers?.Any() == true;
        public int MemberCount => CustomerGroupMembers?.Count ?? 0;

        // Navigation property
        public ICollection<CustomerGroupMember>? CustomerGroupMembers { get; set; }

        // Group Management Methods
        public void AddMember(CustomerGroupMember member)
        {
            CustomerGroupMembers ??= new List<CustomerGroupMember>();
            CustomerGroupMembers.Add(member);
            LastUpdatedAt = DateTime.UtcNow;
        }

        public bool RemoveMember(CustomerGroupMember member)
        {
            var result = CustomerGroupMembers?.Remove(member) ?? false;
            if (result)
            {
                LastUpdatedAt = DateTime.UtcNow;
            }
            return result;
        }


        public bool Equals(CustomerGroup? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return GroupId == other.GroupId &&
                   Name == other.Name;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CustomerGroup)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(GroupId, Name);
        }
    }
}
