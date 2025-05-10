using PosBackend.Application.Dtos.Permissions;
using PosBackend.Models;

namespace PosBackend.Application.Interfaces
{
    public interface IPermissionRepository
    {
        Task<IEnumerable<Permission>> GetAllAsync();
        Task<Permission?> GetByIdAsync(int id);
        Task<Permission?> GetByCodeAsync(string code);
        Task<IEnumerable<Permission>> GetByModuleAsync(string module);
        Task<IEnumerable<string>> GetModulesAsync();
        Task<Permission> CreateAsync(Permission permission);
        Task<bool> UpdateAsync(Permission permission);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(int roleId);
        Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(int userId);
        Task<bool> HasPermissionAsync(int userId, string permissionCode);
    }
}
