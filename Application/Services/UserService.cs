using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PosBackend.Application.Interfaces;
using PosBackend.Models;
using PosBackend.Utilities;

namespace PosBackend.Application.Services
{
    public class UserService : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<UserRole> _roleManager;
        private readonly PosDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IRoleMappingService? _roleMappingService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            PosDbContext context,
            UserManager<User> userManager,
            RoleManager<UserRole> roleManager,
            IPasswordHasher<User> passwordHasher,
            ILogger<UserService> logger,
            IRoleMappingService? roleMappingService = null)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _roleMappingService = roleMappingService;
        }

        public async Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.NormalizedUserName == username.ToUpperInvariant(), cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Looking for user with email: {email}");

            var normalizedEmail = email.ToUpperInvariant();
            _logger.LogInformation($"Normalized email: {normalizedEmail}");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

            if (user != null)
            {
                _logger.LogInformation($"Found user: {user.Id}, Username: {user.UserName}, Email: {user.Email}, NormalizedEmail: {user.NormalizedEmail}, LastLogin: {user.LastLogin}");
            }
            else
            {
                _logger.LogWarning($"No user found with email: {email}");

                // Try a case-insensitive search as a fallback
                var allUsers = await _context.Users.ToListAsync(cancellationToken);
                _logger.LogInformation($"Total users in database: {allUsers.Count}");

                foreach (var u in allUsers)
                {
                    _logger.LogInformation($"User in DB: {u.Id}, Username: {u.UserName}, Email: {u.Email}, NormalizedEmail: {u.NormalizedEmail}");
                }

                var fallbackUser = allUsers.FirstOrDefault(u =>
                    string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

                if (fallbackUser != null)
                {
                    _logger.LogInformation($"Found user with case-insensitive search: {fallbackUser.Id}, Username: {fallbackUser.UserName}, Email: {fallbackUser.Email}");
                    return fallbackUser;
                }
            }

            return user;
        }

        public async Task<User> CreateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task<bool> UpdateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Updating user: {user.Id}, Username: {user.UserName}, Email: {user.Email}, LastLogin: {user.LastLogin}");

                // Check if the entity is being tracked
                var existingUser = await _context.Users.FindAsync(new object[] { user.Id }, cancellationToken);
                if (existingUser != null)
                {
                    _logger.LogInformation($"Found existing user in context: {existingUser.Id}, LastLogin: {existingUser.LastLogin}");

                    // Update specific properties
                    existingUser.LastLogin = user.LastLogin;
                    existingUser.UserName = user.UserName;
                    existingUser.Email = user.Email;
                    existingUser.IsActive = user.IsActive;

                    _logger.LogInformation($"Updated properties in existing user: LastLogin: {existingUser.LastLogin}");
                }
                else
                {
                    _logger.LogInformation($"User not found in context, attaching and updating");
                    _context.Users.Update(user);
                }

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                _logger.LogInformation($"Update result: {result}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user {user.Id}");
                throw;
            }
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

            // If we have the role mapping service, try to sync with Keycloak
            if (result.Succeeded && _roleMappingService != null && !string.IsNullOrEmpty(user.SecurityStamp))
            {
                try
                {
                    // Use the security stamp as the Keycloak user ID (this is a simplification)
                    // In a real implementation, you would store the Keycloak user ID in a separate field
                    await _roleMappingService.SynchronizeUserRolesAsync(user.SecurityStamp, userId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to synchronize roles with Keycloak for user {UserId}", userId);
                    // We don't fail the operation if Keycloak sync fails
                }
            }

            return result.Succeeded;
        }

        public async Task<bool> RemoveUserFromRoleAsync(int userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);

            // If we have the role mapping service, try to sync with Keycloak
            if (result.Succeeded && _roleMappingService != null && !string.IsNullOrEmpty(user.SecurityStamp))
            {
                try
                {
                    // Use the security stamp as the Keycloak user ID (this is a simplification)
                    await _roleMappingService.SynchronizeUserRolesAsync(user.SecurityStamp, userId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to synchronize roles with Keycloak for user {UserId}", userId);
                    // We don't fail the operation if Keycloak sync fails
                }
            }

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