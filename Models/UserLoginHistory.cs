using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosBackend.Models
{
    public class UserLoginHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LoginId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTime LoginTime { get; set; }

        public DateTime? LogoutTime { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [MaxLength(255)]
        public string? UserAgent { get; set; }

        [MaxLength(100)]
        public string? DeviceInfo { get; set; }

        public bool IsSuccessful { get; set; }

        [MaxLength(255)]
        public string? FailureReason { get; set; }

        public UserLoginHistory()
        {
            LoginTime = DateTime.UtcNow;
        }
    }
}