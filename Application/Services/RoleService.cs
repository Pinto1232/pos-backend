using Microsoft.EntityFrameworkCore;
using PosBackend.Application.Interfaces;
using PosBackend.Models;
using Microsoft.AspNetCore.Identity;

namespace PosBackend.Application.Services
{
    public class RoleService : IRoleRepository
    {
        private readonly PosDbContext _context;

        public RoleService(PosDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserRole>> GetAllAsync()
        {
            return await _context.Roles
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<UserRole?> GetByIdAsync(int id)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<UserRole?> GetByNameAsync(string name)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == name);
        }

        public async Task<UserRole> CreateAsync(UserRole role)
        {
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<bool> UpdateAsync(UserRole role)
        {
            _context.Entry(role).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await RoleExistsAsync(role.Id))
                {
                    return false;
                }
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return false;
            }

            // Check if role is in use
            var userCount = await GetUserCountByRoleIdAsync(id);
            if (userCount > 0)
            {
                throw new InvalidOperationException($"Cannot delete role '{role.Name}' because it is assigned to {userCount} users.");
            }

            // Remove role permissions first
            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == id)
                .ToListAsync();

            _context.RolePermissions.RemoveRange(rolePermissions);
            _context.Roles.Remove(role);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId)
        {
            // Check if role and permission exist
            var role = await _context.Roles.FindAsync(roleId);
            var permission = await _context.Permissions.FindAsync(permissionId);

            if (role == null || permission == null)
            {
                return false;
            }

            // Check if assignment already exists
            var exists = await _context.RolePermissions
                .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (exists)
            {
                return true; // Already assigned
            }

            // Create new assignment
            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            };

            _context.RolePermissions.Add(rolePermission);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId)
        {
            var rolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (rolePermission == null)
            {
                return false;
            }

            _context.RolePermissions.Remove(rolePermission);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignRoleToUserAsync(int roleId, int userId)
        {
            // Check if role and user exist
            var role = await _context.Roles.FindAsync(roleId);
            var user = await _context.Users.FindAsync(userId);

            if (role == null || user == null)
            {
                return false;
            }

            // Check if assignment already exists
            var exists = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (exists)
            {
                return true; // Already assigned
            }

            // Create new assignment
            var userRole = new UserRoleMapping
            {
                UserId = userId,
                RoleId = roleId
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveRoleFromUserAsync(int roleId, int userId)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (userRole == null)
            {
                return false;
            }

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUserCountByRoleIdAsync(int roleId)
        {
            return await _context.UserRoles
                .CountAsync(ur => ur.RoleId == roleId);
        }

        private async Task<bool> RoleExistsAsync(int id)
        {
            return await _context.Roles.AnyAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<UserRole>> GetUserRolesAsync(int userId)
        {
            var userRoleMappings = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            var roleIds = userRoleMappings.Select(ur => ur.RoleId).ToList();

            return await _context.Roles
                .Where(r => roleIds.Contains(r.Id))
                .ToListAsync();
        }
    }
}
