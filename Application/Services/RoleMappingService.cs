using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PosBackend.Application.Interfaces;
using PosBackend.Models;

namespace PosBackend.Application.Services
{
    public class RoleMappingService : IRoleMappingService
    {
        private readonly PosDbContext _context;
        private readonly IKeycloakService _keycloakService;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<RoleMappingService> _logger;

        public RoleMappingService(
            PosDbContext context,
            IKeycloakService keycloakService,
            IPermissionRepository permissionRepository,
            IRoleRepository roleRepository,
            ILogger<RoleMappingService> logger)
        {
            _context = context;
            _keycloakService = keycloakService;
            _permissionRepository = permissionRepository;
            _roleRepository = roleRepository;
            _logger = logger;
        }

        /// <summary>
        /// Synchronizes Keycloak roles with application roles
        /// </summary>
        public async Task SynchronizeRolesAsync()
        {
            try
            {
                // Get all roles from Keycloak
                var keycloakRoles = await _keycloakService.GetRealmRolesAsync();
                
                // Get all roles from the application
                var appRoles = await _roleRepository.GetAllAsync();
                
                // Create application roles for Keycloak roles that don't exist in the application
                foreach (var keycloakRole in keycloakRoles)
                {
                    // Skip default Keycloak roles
                    if (IsDefaultKeycloakRole(keycloakRole.Name))
                    {
                        continue;
                    }

                    var appRole = appRoles.FirstOrDefault(r => r.Name == keycloakRole.Name);
                    if (appRole == null)
                    {
                        _logger.LogInformation($"Creating application role for Keycloak role: {keycloakRole.Name}");
                        
                        var newRole = new UserRole
                        {
                            Name = keycloakRole.Name,
                            NormalizedName = keycloakRole.Description ?? keycloakRole.Name.ToUpper(),
                            ConcurrencyStamp = Guid.NewGuid().ToString()
                        };
                        
                        await _roleRepository.CreateAsync(newRole);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing roles from Keycloak");
                throw;
            }
        }

        /// <summary>
        /// Maps Keycloak roles to application permissions
        /// </summary>
        public async Task MapRoleToPermissionsAsync(string roleName, List<string> permissionCodes)
        {
            try
            {
                // Get the role from the application
                var role = await _roleRepository.GetByNameAsync(roleName);
                if (role == null)
                {
                    _logger.LogWarning($"Role '{roleName}' not found in the application");
                    return;
                }

                // Get the permissions from the application
                var permissions = new List<Permission>();
                foreach (var code in permissionCodes)
                {
                    var permission = await _permissionRepository.GetByCodeAsync(code);
                    if (permission != null)
                    {
                        permissions.Add(permission);
                    }
                    else
                    {
                        _logger.LogWarning($"Permission with code '{code}' not found");
                    }
                }

                // Remove existing role permissions
                var existingRolePermissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == role.Id)
                    .ToListAsync();
                
                _context.RolePermissions.RemoveRange(existingRolePermissions);
                
                // Add new role permissions
                foreach (var permission in permissions)
                {
                    var rolePermission = new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.Id
                    };
                    
                    _context.RolePermissions.Add(rolePermission);
                }
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Mapped {permissions.Count} permissions to role '{roleName}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error mapping permissions to role '{roleName}'");
                throw;
            }
        }

        /// <summary>
        /// Synchronizes user roles from Keycloak to the application
        /// </summary>
        public async Task SynchronizeUserRolesAsync(string keycloakUserId, int appUserId)
        {
            try
            {
                // Get user roles from Keycloak
                var keycloakRoles = await _keycloakService.GetUserRolesAsync(keycloakUserId);
                
                // Get all application roles
                var appRoles = await _roleRepository.GetAllAsync();
                
                // Get user's current roles in the application
                var userRoles = await _roleRepository.GetUserRolesAsync(appUserId);
                
                // Remove roles that are not in Keycloak
                foreach (var userRole in userRoles)
                {
                    if (userRole.Name != null && !keycloakRoles.Any(kr => kr.Name == userRole.Name) && !IsDefaultKeycloakRole(userRole.Name))
                    {
                        await _roleRepository.RemoveRoleFromUserAsync(appUserId, userRole.Id);
                        _logger.LogInformation($"Removed role '{userRole.Name}' from user {appUserId}");
                    }
                }
                
                // Add roles from Keycloak that are not in the application
                foreach (var keycloakRole in keycloakRoles)
                {
                    // Skip default Keycloak roles
                    if (IsDefaultKeycloakRole(keycloakRole.Name))
                    {
                        continue;
                    }
                    
                    var appRole = appRoles.FirstOrDefault(r => r.Name == keycloakRole.Name);
                    if (appRole != null && !userRoles.Any(ur => ur.Id == appRole.Id))
                    {
                        await _roleRepository.AssignRoleToUserAsync(appRole.Id, appUserId);
                        _logger.LogInformation($"Added role '{appRole.Name}' to user {appUserId}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error synchronizing roles for user {appUserId} from Keycloak");
                throw;
            }
        }

        /// <summary>
        /// Checks if a role is a default Keycloak role that should be skipped
        /// </summary>
        private bool IsDefaultKeycloakRole(string roleName)
        {
            var defaultRoles = new[] 
            { 
                "default-roles", 
                "offline_access", 
                "uma_authorization",
                "create-realm",
                "admin",
                "create-client"
            };
            
            return defaultRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
        }
    }
}
