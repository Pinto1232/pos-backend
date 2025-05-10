using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class UserPermission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int PermissionId { get; set; }

        // Indicates if this is an override (grant or deny)
        public bool IsGranted { get; set; } = true;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; } = null!;
    }
}
