using System.Collections.Generic;
using System.Threading.Tasks;
using PosBackend.Models;

namespace PosBackend.Application.Interfaces
{
    public interface IKeycloakService
    {
        /// <summary>
        /// Gets all roles from Keycloak realm
        /// </summary>
        Task<List<KeycloakRole>> GetRealmRolesAsync();

        /// <summary>
        /// Gets user roles from Keycloak
        /// </summary>
        Task<List<KeycloakRole>> GetUserRolesAsync(string userId);

        /// <summary>
        /// Assigns a role to a user in Keycloak
        /// </summary>
        Task AssignRoleToUserAsync(string userId, string roleName);

        /// <summary>
        /// Gets all users from Keycloak realm
        /// </summary>
        Task<List<KeycloakUser>> GetAllUsersAsync();
    }
}
