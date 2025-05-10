using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosBackend.Application.Interfaces;
using PosBackend.Middlewares;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoleMappingsController : ControllerBase
    {
        private readonly IRoleMappingService _roleMappingService;
        private readonly IKeycloakService _keycloakService;
        private readonly ILogger<RoleMappingsController> _logger;

        public RoleMappingsController(
            IRoleMappingService roleMappingService,
            IKeycloakService keycloakService,
            ILogger<RoleMappingsController> logger)
        {
            _roleMappingService = roleMappingService;
            _keycloakService = keycloakService;
            _logger = logger;
        }

        // POST: api/RoleMappings/sync
        [HttpPost("sync")]
        public async Task<IActionResult> SynchronizeRoles()
        {
            try
            {
                await _roleMappingService.SynchronizeRolesAsync();
                return Ok(new { message = "Roles synchronized successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing roles");
                return StatusCode(500, new { error = "Failed to synchronize roles", message = ex.Message });
            }
        }

        // POST: api/RoleMappings/map
        [HttpPost("map")]
        [RequireRole("Administrator")]
        public async Task<IActionResult> MapRoleToPermissions([FromBody] RolePermissionMappingRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RoleName))
                {
                    return BadRequest(new { error = "Role name is required" });
                }

                await _roleMappingService.MapRoleToPermissionsAsync(request.RoleName, request.PermissionCodes);
                return Ok(new { message = $"Permissions mapped to role '{request.RoleName}' successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping permissions to role");
                return StatusCode(500, new { error = "Failed to map permissions to role", message = ex.Message });
            }
        }

        // POST: api/RoleMappings/user/{userId}/sync
        [HttpPost("user/{userId}/sync")]
        [RequireRole("Administrator")]
        public async Task<IActionResult> SynchronizeUserRoles(int userId, [FromBody] KeycloakUserIdRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.KeycloakUserId))
                {
                    return BadRequest(new { error = "Keycloak user ID is required" });
                }

                await _roleMappingService.SynchronizeUserRolesAsync(request.KeycloakUserId, userId);
                return Ok(new { message = $"Roles synchronized for user {userId} successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing user roles");
                return StatusCode(500, new { error = "Failed to synchronize user roles", message = ex.Message });
            }
        }

        // GET: api/RoleMappings/keycloak/roles
        [HttpGet("keycloak/roles")]
        public async Task<IActionResult> GetKeycloakRoles()
        {
            try
            {
                var roles = await _keycloakService.GetRealmRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Keycloak roles");
                return StatusCode(500, new { error = "Failed to get Keycloak roles", message = ex.Message });
            }
        }
    }

    public class RolePermissionMappingRequest
    {
        public string RoleName { get; set; } = string.Empty;
        public List<string> PermissionCodes { get; set; } = new List<string>();
    }

    public class KeycloakUserIdRequest
    {
        public string KeycloakUserId { get; set; } = string.Empty;
    }
}
