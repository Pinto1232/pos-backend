using Microsoft.EntityFrameworkCore;
using PosBackend.Application.Interfaces;
using PosBackend.Models;

namespace PosBackend.Application.Services
{
    public class PermissionService : IPermissionRepository
    {
        private readonly PosDbContext _context;

        public PermissionService(PosDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Permission>> GetAllAsync()
        {
            return await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Permission?> GetByIdAsync(int id)
        {
            return await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Permission?> GetByCodeAsync(string code)
        {
            return await _context.Permissions
                .FirstOrDefaultAsync(p => p.Code == code);
        }

        public async Task<IEnumerable<Permission>> GetByModuleAsync(string module)
        {
            return await _context.Permissions
                .Where(p => p.Module == module && p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetModulesAsync()
        {
            return await _context.Permissions
                .Where(p => p.IsActive)
                .Select(p => p.Module)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();
        }

        public async Task<Permission> CreateAsync(Permission permission)
        {
            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();
            return permission;
        }

        public async Task<bool> UpdateAsync(Permission permission)
        {
            _context.Entry(permission).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PermissionExistsAsync(permission.Id))
                {
                    return false;
                }
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                return false;
            }

            // Soft delete by setting IsActive to false
            permission.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(int roleId)
        {
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Include(rp => rp.Permission)
                .Where(rp => rp.Permission.IsActive)
                .Select(rp => rp.Permission)
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(int userId)
        {
            // Get permissions directly assigned to the user
            var userPermissions = await _context.UserPermissions
                .Where(up => up.UserId == userId && up.IsGranted)
                .Include(up => up.Permission)
                .Where(up => up.Permission.IsActive)
                .Select(up => up.Permission)
                .ToListAsync();

            // Get permissions from user's roles
            var roleIds = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var rolePermissions = await _context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Include(rp => rp.Permission)
                .Where(rp => rp.Permission.IsActive)
                .Select(rp => rp.Permission)
                .ToListAsync();

            // Combine and remove duplicates
            return userPermissions
                .Union(rolePermissions, new PermissionComparer())
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Name)
                .ToList();
        }

        public async Task<bool> HasPermissionAsync(int userId, string permissionCode)
        {
            // Check if user has this permission directly assigned
            var hasDirectPermission = await _context.UserPermissions
                .AnyAsync(up => up.UserId == userId &&
                                up.IsGranted &&
                                up.Permission.Code == permissionCode &&
                                up.Permission.IsActive);

            if (hasDirectPermission)
            {
                return true;
            }

            // Check if user has this permission through roles
            var roleIds = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            return await _context.RolePermissions
                .AnyAsync(rp => roleIds.Contains(rp.RoleId) &&
                               rp.Permission.Code == permissionCode &&
                               rp.Permission.IsActive);
        }

        private async Task<bool> PermissionExistsAsync(int id)
        {
            return await _context.Permissions.AnyAsync(p => p.Id == id);
        }
    }

    // Helper class for comparing permissions
    public class PermissionComparer : IEqualityComparer<Permission>
    {
        public bool Equals(Permission? x, Permission? y)
        {
            if (x == null || y == null)
                return false;

            return x.Id == y.Id;
        }

        public int GetHashCode(Permission obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
