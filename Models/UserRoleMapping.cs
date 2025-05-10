using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class UserRoleMapping
    {
        [Key]
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public int RoleId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        [ForeignKey("RoleId")]
        public virtual UserRole? Role { get; set; }
    }
}
