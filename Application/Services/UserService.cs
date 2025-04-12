using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PosBackend.Application.Interfaces;
using PosBackend.Models;

namespace PosBackend.Application.Services
{
    public class UserService : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<UserRole> _roleManager;
        private readonly PosDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(
            PosDbContext context,
            UserManager<User> userManager,
            RoleManager<UserRole> roleManager,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _passwordHasher = passwordHasher;
        }

        public async Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.NormalizedUserName == username.ToUpperInvariant(), cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpperInvariant(), cancellationToken);
        }

        public async Task<User> CreateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task<bool> UpdateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Update(user);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }

        public async Task<bool> DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> CheckPasswordAsync(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);
            return user != null && await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result.Succeeded;
        }

        public async Task<bool> IsUserExistsAsync(string username)
        {
            return await _userManager.FindByNameAsync(username) != null;
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) == null;
        }

        public async Task<IEnumerable<UserRole>> GetUserRolesAsync(int userId)
        {
            var roleIds = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            return await _context.Roles
                .Where(r => roleIds.Contains(r.Id))
                .ToListAsync();
        }

        public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var role = await _roleManager.FindByIdAsync(roleId.ToString());

            if (user == null || role == null || role.Name == null)
                return false;

            var result = await _userManager.AddToRoleAsync(user, role.Name);
            return result.Succeeded;
        }

        public async Task<bool> RemoveUserFromRoleAsync(int userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<IEnumerable<UserLoginHistory>> GetUserLoginHistoryAsync(int userId, int count = 10)
        {
            return await _context.UserLoginHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.LoginTime)
                .Take(count)
                .ToListAsync();
        }

        public async Task LogUserLoginAttemptAsync(string username, bool isSuccessful, string ipAddress)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return;

            await _context.UserLoginHistories.AddAsync(new UserLoginHistory
            {
                UserId = user.Id,
                LoginTime = DateTime.UtcNow,
                IsSuccessful = isSuccessful,
                IpAddress = ipAddress ?? "Unknown"
            });
            await _context.SaveChangesAsync();
        }
    }
}