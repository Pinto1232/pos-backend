using PosBackend.Models;

namespace PosBackend.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}