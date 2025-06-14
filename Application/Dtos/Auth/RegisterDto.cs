using System.ComponentModel.DataAnnotations;
using PosBackend.Attributes;

public class RegisterDto
{
    [Required]
    [SafeUsername]
    public required string Username { get; set; }
    
    [Required]
    [SafeEmail]
    public required string Email { get; set; }
    
    [Required]
    [StrongPassword]
    public required string Password { get; set; }
}