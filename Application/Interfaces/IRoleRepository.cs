using PosBackend.Models;
using System.Threading.Tasks;

namespace PosBackend.Application.Interfaces
{
    public interface IRoleRepository
    {
        Task<UserRole> GetByNameAsync(string roleName);
    }
}