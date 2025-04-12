using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace PosBackend.Models
{
    [Index(nameof(Username), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class User : IdentityUser<int>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        // Use "new" keyword to explicitly hide the base class property
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public new required string Email { get; set; } = string.Empty;
        
        [Required]
        public new required string PasswordHash { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation property for user roles
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<UserLoginHistory> LoginHistories { get; set; } = new List<UserLoginHistory>();
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}