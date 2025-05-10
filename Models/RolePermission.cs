using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class RolePermission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        public int PermissionId { get; set; }

        // Navigation properties
        [ForeignKey("RoleId")]
        public virtual UserRole Role { get; set; } = null!;

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; } = null!;
    }
}
