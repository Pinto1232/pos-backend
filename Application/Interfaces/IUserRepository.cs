using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using PosBackend.Models;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using PosBackend.Application.Interfaces;
using PosBackend.Application.Services;
using System.Linq;
using System;

namespace PosBackend.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User> CreateUserAsync(User user, CancellationToken cancellationToken = default);
        Task<bool> UpdateUserAsync(User user, CancellationToken cancellationToken = default);
        Task<bool> DeleteUserAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> CheckPasswordAsync(string username, string password);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> IsUserExistsAsync(string username);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<IEnumerable<UserRole>> GetUserRolesAsync(int userId);
        Task<bool> AssignRoleToUserAsync(int userId, int roleId);
        Task<bool> RemoveUserFromRoleAsync(int userId, string roleName);
        Task<IEnumerable<UserLoginHistory>> GetUserLoginHistoryAsync(int userId, int count = 10);
        Task LogUserLoginAttemptAsync(string username, bool isSuccessful, string ipAddress);
    }
}