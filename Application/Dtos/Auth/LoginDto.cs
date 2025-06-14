using System.ComponentModel.DataAnnotations;
using PosBackend.Attributes;
using PosBackend.Services;

public class LoginDto
{
    [Required]
    [SanitizeString(InputType.Username, 50)]
    public required string Username { get; set; }
    
    [Required]
    [SanitizeString(InputType.Password, 128)]
    public required string Password { get; set; }
}