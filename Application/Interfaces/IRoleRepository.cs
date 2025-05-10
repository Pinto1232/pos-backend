using PosBackend.Application.Dtos.Roles;
using PosBackend.Models;

namespace PosBackend.Application.Interfaces
{
    public interface IRoleRepository
    {
        Task<IEnumerable<UserRole>> GetAllAsync();
        Task<UserRole?> GetByIdAsync(int id);
        Task<UserRole?> GetByNameAsync(string name);
        Task<UserRole> CreateAsync(UserRole role);
        Task<bool> UpdateAsync(UserRole role);
        Task<bool> DeleteAsync(int id);
        Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId);
        Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId);
        Task<bool> AssignRoleToUserAsync(int roleId, int userId);
        Task<bool> RemoveRoleFromUserAsync(int roleId, int userId);
        Task<int> GetUserCountByRoleIdAsync(int roleId);
        Task<IEnumerable<UserRole>> GetUserRolesAsync(int userId);
    }
}