namespace PosBackend.Application.DTOs
{
    public class UserRegistrationDto
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }

        public UserRegistrationDto(string username, string email, string password)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Password = password ?? throw new ArgumentNullException(nameof(password));
        }
    }


    public class UserLoginDto
    {
        public required string Username { get; set; }
        public required string Password { get; set; }

        public UserLoginDto(string username, string password)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Password = password ?? throw new ArgumentNullException(nameof(password));
        }
    }


    public class UserResponseDto
    {
        public int UserId { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class AuthenticationResult
    {
        public required string Token { get; set; }
        public UserResponseDto? User { get; set; }
    }
}
