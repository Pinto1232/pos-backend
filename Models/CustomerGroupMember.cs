using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class CustomerGroupMember
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MembershipId { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public required Customer Customer { get; set; }

        [ForeignKey("CustomerGroup")]
        public int GroupId { get; set; }
        public required CustomerGroup CustomerGroup { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
