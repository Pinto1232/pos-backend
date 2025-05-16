using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PosBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class KeycloakPermissionsController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<KeycloakPermissionsController> _logger;

        public KeycloakPermissionsController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<KeycloakPermissionsController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPermissions()
        {
            try
            {
                var keycloakAuthority = _configuration["Keycloak:Authority"];
                var clientId = _configuration["Keycloak:ClientId"];

                if (string.IsNullOrEmpty(keycloakAuthority) || string.IsNullOrEmpty(clientId))
                {
                    return BadRequest("Keycloak configuration is missing");
                }

                // Get the access token from the request
                var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Create HTTP client
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Get resources from Keycloak
                var resourcesUrl = $"{keycloakAuthority}/authz/protection/resource_set";
                var resourcesResponse = await client.GetAsync(resourcesUrl);

                if (!resourcesResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to get resources from Keycloak: {resourcesResponse.StatusCode}");
                    return StatusCode((int)resourcesResponse.StatusCode, "Failed to get resources from Keycloak");
                }

                var resourcesContent = await resourcesResponse.Content.ReadAsStringAsync();
                var resources = JsonSerializer.Deserialize<List<KeycloakResource>>(resourcesContent) ?? new List<KeycloakResource>();

                // Map resources to permissions
                var permissions = resources.Select(r => new
                {
                    id = r.Id,
                    name = r.Name,
                    displayName = r.DisplayName ?? r.Name,
                    category = r.Type ?? "Default",
                    description = r.Description
                }).ToList();

                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions from Keycloak");
                return StatusCode(500, "Error getting permissions from Keycloak");
            }
        }

        [HttpGet("Role/{roleId}")]
        public async Task<IActionResult> GetRolePermissions(int roleId)
        {
            try
            {
                var keycloakAuthority = _configuration["Keycloak:Authority"];
                var clientId = _configuration["Keycloak:ClientId"];

                if (string.IsNullOrEmpty(keycloakAuthority) || string.IsNullOrEmpty(clientId))
                {
                    return BadRequest("Keycloak configuration is missing");
                }

                // Get the access token from the request
                var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Create HTTP client
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Get role from database to get the Keycloak role ID
                // This is a placeholder - you would need to implement this based on your database schema
                var keycloakRoleId = await GetKeycloakRoleId(roleId);

                if (string.IsNullOrEmpty(keycloakRoleId))
                {
                    return NotFound($"Role with ID {roleId} not found");
                }

                // Get role permissions from Keycloak
                var rolePermissionsUrl = $"{keycloakAuthority}/admin/realms/{_configuration["Keycloak:Realm"]}/roles/{keycloakRoleId}/permissions";
                var rolePermissionsResponse = await client.GetAsync(rolePermissionsUrl);

                if (!rolePermissionsResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to get role permissions from Keycloak: {rolePermissionsResponse.StatusCode}");
                    return StatusCode((int)rolePermissionsResponse.StatusCode, "Failed to get role permissions from Keycloak");
                }

                var rolePermissionsContent = await rolePermissionsResponse.Content.ReadAsStringAsync();
                var rolePermissions = JsonSerializer.Deserialize<List<string>>(rolePermissionsContent);

                return Ok(rolePermissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting permissions for role {roleId} from Keycloak");
                return StatusCode(500, $"Error getting permissions for role {roleId} from Keycloak");
            }
        }

        [HttpPut("Role/{roleId}")]
        public async Task<IActionResult> UpdateRolePermissions(int roleId, [FromBody] List<string> permissions)
        {
            try
            {
                var keycloakAuthority = _configuration["Keycloak:Authority"];
                var clientId = _configuration["Keycloak:ClientId"];

                if (string.IsNullOrEmpty(keycloakAuthority) || string.IsNullOrEmpty(clientId))
                {
                    return BadRequest("Keycloak configuration is missing");
                }

                // Get the access token from the request
                var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Create HTTP client
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Get role from database to get the Keycloak role ID
                var keycloakRoleId = await GetKeycloakRoleId(roleId);

                if (string.IsNullOrEmpty(keycloakRoleId))
                {
                    return NotFound($"Role with ID {roleId} not found");
                }

                // Update role permissions in Keycloak
                var updateRolePermissionsUrl = $"{keycloakAuthority}/admin/realms/{_configuration["Keycloak:Realm"]}/roles/{keycloakRoleId}/permissions";
                var content = new StringContent(JsonSerializer.Serialize(permissions), Encoding.UTF8, "application/json");
                var updateRolePermissionsResponse = await client.PutAsync(updateRolePermissionsUrl, content);

                if (!updateRolePermissionsResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to update role permissions in Keycloak: {updateRolePermissionsResponse.StatusCode}");
                    return StatusCode((int)updateRolePermissionsResponse.StatusCode, "Failed to update role permissions in Keycloak");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating permissions for role {roleId} in Keycloak");
                return StatusCode(500, $"Error updating permissions for role {roleId} in Keycloak");
            }
        }

        [HttpGet("Check/{permission}")]
        public async Task<IActionResult> CheckPermission(string permission)
        {
            try
            {
                // Get the access token from the request
                var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized("No access token provided");
                }

                // Parse the permission string (e.g., "users.view" -> resource="users", scope="view")
                var parts = permission.Split('.');
                var resource = parts[0];
                var scope = parts.Length > 1 ? parts[1] : string.Empty; // Use empty string instead of null

                // Get the user ID from the token claims
                var userId = User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                // Use the KeycloakAuthorizationService to check the permission
                var keycloakAuthzService = HttpContext.RequestServices.GetRequiredService<Services.KeycloakAuthorizationService>();
                var hasPermission = await keycloakAuthzService.HasPermissionAsync(userId, resource, scope, accessToken);

                return Ok(new { hasPermission });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking permission {permission}");
                return StatusCode(500, $"Error checking permission {permission}");
            }
        }

        // Helper method to get Keycloak role ID from database
        private Task<string> GetKeycloakRoleId(int roleId)
        {
            // This is a placeholder - you would need to implement this based on your database schema
            // For now, we'll return a mock ID
            // Changed to non-async method that returns a completed task to avoid CS1998 warning
            return Task.FromResult($"role-{roleId}");
        }
    }

    // Model classes for Keycloak resources
    public class KeycloakResource
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public required string Type { get; set; }
        public required string Description { get; set; }
        public required List<string> Scopes { get; set; }
    }
}
