using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace PosBackend.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class User : IdentityUser<int>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public override string? Email { get; set; } = string.Empty;

        [Required]
        public override string? PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<UserLoginHistory> LoginHistories { get; set; } = new List<UserLoginHistory>();
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}
