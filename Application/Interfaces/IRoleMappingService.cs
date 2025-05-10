using System.Collections.Generic;
using System.Threading.Tasks;

namespace PosBackend.Application.Interfaces
{
    public interface IRoleMappingService
    {
        /// <summary>
        /// Synchronizes Keycloak roles with application roles
        /// </summary>
        Task SynchronizeRolesAsync();

        /// <summary>
        /// Maps Keycloak roles to application permissions
        /// </summary>
        Task MapRoleToPermissionsAsync(string roleName, List<string> permissionCodes);

        /// <summary>
        /// Synchronizes user roles from Keycloak to the application
        /// </summary>
        Task SynchronizeUserRolesAsync(string keycloakUserId, int appUserId);
    }
}
