using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosBackend.Application.Interfaces;
using PosBackend.Middlewares;
using PosBackend.Utilities;
using System;
using System.Threading.Tasks;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class KeycloakRolesController : ControllerBase
    {
        private readonly IKeycloakService _keycloakService;
        private readonly ILogger<KeycloakRolesController> _logger;

        public KeycloakRolesController(
            IKeycloakService keycloakService,
            ILogger<KeycloakRolesController> logger)
        {
            _keycloakService = keycloakService;
            _logger = logger;
        }

        // GET: api/KeycloakRoles
        [HttpGet]
        public async Task<IActionResult> GetRoles()
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

        // GET: api/KeycloakRoles/current
        [HttpGet("current")]
        public IActionResult GetCurrentUserRoles()
        {
            try
            {
                var token = JwtTokenUtils.ExtractTokenFromRequest(Request);
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new { error = "No token found in request" });
                }

                var roles = JwtTokenUtils.ExtractRolesFromToken(token);
                return Ok(new { roles });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user roles");
                return StatusCode(500, new { error = "Failed to get current user roles", message = ex.Message });
            }
        }

        // GET: api/KeycloakRoles/user/{userId}
        [HttpGet("user/{userId}")]
        [RequireRole("Administrator")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            try
            {
                var roles = await _keycloakService.GetUserRolesAsync(userId);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles");
                return StatusCode(500, new { error = "Failed to get user roles", message = ex.Message });
            }
        }

        // POST: api/KeycloakRoles/user/{userId}/role/{roleName}
        [HttpPost("user/{userId}/role/{roleName}")]
        [RequireRole("Administrator")]
        public async Task<IActionResult> AssignRoleToUser(string userId, string roleName)
        {
            try
            {
                await _keycloakService.AssignRoleToUserAsync(userId, roleName);
                return Ok(new { message = $"Role '{roleName}' assigned to user '{userId}' successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role to user");
                return StatusCode(500, new { error = "Failed to assign role to user", message = ex.Message });
            }
        }
    }
}
