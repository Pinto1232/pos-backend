using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using PosBackend.Application.Services.Caching;
using KeycloakCacheKeys = PosBackend.Services.CacheKeys;

namespace PosBackend.Services
{
    public class KeycloakAuthorizationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<KeycloakAuthorizationService> _logger;
        private readonly ICacheService _cacheService;

        public KeycloakAuthorizationService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<KeycloakAuthorizationService> logger,
            ICacheService cacheService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _cacheService = cacheService;
        }

        /// <summary>
        /// Get all resources from Keycloak
        /// </summary>
        public async Task<List<KeycloakResource>> GetAllResourcesAsync(string accessToken)
        {
            var cacheKey = KeycloakCacheKeys.KeycloakResources();

            // Try to get from cache first
            var cachedResources = _cacheService.Get<List<KeycloakResource>>(cacheKey);
            if (cachedResources != null)
            {
                return cachedResources;
            }

            try
            {
                var keycloakAuthority = _configuration["Keycloak:Authority"];
                var clientId = _configuration["Keycloak:ClientId"];

                if (string.IsNullOrEmpty(keycloakAuthority) || string.IsNullOrEmpty(clientId))
                {
                    throw new InvalidOperationException("Keycloak configuration is missing");
                }

                // Create HTTP client
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Get resources from Keycloak
                var resourcesUrl = $"{keycloakAuthority}/authz/protection/resource_set";
                var resourcesResponse = await client.GetAsync(resourcesUrl);

                resourcesResponse.EnsureSuccessStatusCode();

                var resourcesContent = await resourcesResponse.Content.ReadAsStringAsync();
                var resources = JsonSerializer.Deserialize<List<KeycloakResource>>(resourcesContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<KeycloakResource>();

                // Cache the resources
                await _cacheService.SetAsync(cacheKey, resources, DateTimeOffset.Now.AddMinutes(10));

                return resources;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resources from Keycloak");
                throw;
            }
        }

        /// <summary>
        /// Get all permissions for a role from Keycloak
        /// </summary>
        public async Task<List<string>> GetRolePermissionsAsync(string roleName, string accessToken)
        {
            var cacheKey = KeycloakCacheKeys.KeycloakRolePermissions(roleName);

            // Try to get from cache first
            var cachedPermissions = _cacheService.Get<List<string>>(cacheKey);
            if (cachedPermissions != null)
            {
                return cachedPermissions;
            }

            try
            {
                var keycloakAuthority = _configuration["Keycloak:Authority"];
                var realm = _configuration["Keycloak:Realm"];

                if (string.IsNullOrEmpty(keycloakAuthority) || string.IsNullOrEmpty(realm))
                {
                    throw new InvalidOperationException("Keycloak configuration is missing");
                }

                // Create HTTP client
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Get role permissions from Keycloak
                var rolePermissionsUrl = $"{keycloakAuthority}/admin/realms/{realm}/roles/{roleName}/permissions";
                var rolePermissionsResponse = await client.GetAsync(rolePermissionsUrl);

                rolePermissionsResponse.EnsureSuccessStatusCode();

                var rolePermissionsContent = await rolePermissionsResponse.Content.ReadAsStringAsync();
                var rolePermissions = JsonSerializer.Deserialize<List<string>>(rolePermissionsContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<string>();

                // Cache the permissions
                await _cacheService.SetAsync(cacheKey, rolePermissions, DateTimeOffset.Now.AddMinutes(5));

                return rolePermissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting permissions for role {roleName} from Keycloak");
                throw;
            }
        }

        /// <summary>
        /// Update permissions for a role in Keycloak
        /// </summary>
        public async Task UpdateRolePermissionsAsync(string roleName, List<string> permissions, string accessToken)
        {
            try
            {
                var keycloakAuthority = _configuration["Keycloak:Authority"];
                var realm = _configuration["Keycloak:Realm"];

                if (string.IsNullOrEmpty(keycloakAuthority) || string.IsNullOrEmpty(realm))
                {
                    throw new InvalidOperationException("Keycloak configuration is missing");
                }

                // Create HTTP client
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Update role permissions in Keycloak
                var updateRolePermissionsUrl = $"{keycloakAuthority}/admin/realms/{realm}/roles/{roleName}/permissions";
                var content = new StringContent(JsonSerializer.Serialize(permissions), Encoding.UTF8, "application/json");
                var updateRolePermissionsResponse = await client.PutAsync(updateRolePermissionsUrl, content);

                updateRolePermissionsResponse.EnsureSuccessStatusCode();

                // Invalidate cache
                await _cacheService.RemoveAsync(KeycloakCacheKeys.KeycloakRolePermissions(roleName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating permissions for role {roleName} in Keycloak");
                throw;
            }
        }

        /// <summary>
        /// Check if a user has a specific permission
        /// </summary>
        public async Task<bool> HasPermissionAsync(string userId, string resource, string scope, string accessToken)
        {
            try
            {
                var keycloakAuthority = _configuration["Keycloak:Authority"];
                var clientId = _configuration["Keycloak:ClientId"];

                if (string.IsNullOrEmpty(keycloakAuthority) || string.IsNullOrEmpty(clientId))
                {
                    throw new InvalidOperationException("Keycloak configuration is missing");
                }

                // Create HTTP client
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Check permission using Keycloak's evaluation endpoint
                var checkPermissionUrl = $"{keycloakAuthority}/authz/protection/permission/evaluate";

                var requestBody = new
                {
                    resources = new[] { resource },
                    scopes = string.IsNullOrEmpty(scope) ? Array.Empty<string>() : new[] { scope },
                    userId = userId
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var checkPermissionResponse = await client.PostAsync(checkPermissionUrl, content);

                checkPermissionResponse.EnsureSuccessStatusCode();

                var checkPermissionContent = await checkPermissionResponse.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<KeycloakPermissionEvaluationResult>(checkPermissionContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.Result ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking permission for user {userId}, resource {resource}, scope {scope}");
                return false;
            }
        }
    }

    // Model classes for Keycloak resources and permissions
    public class KeycloakResource
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public required string Type { get; set; }
        public required string Description { get; set; }
        public required List<string> Scopes { get; set; }
    }

    public class KeycloakPermissionEvaluationResult
    {
        public bool Result { get; set; }
    }

    // Cache keys
    public static class CacheKeys
    {
        public static string KeycloakResources() => "keycloak:resources";
        public static string KeycloakRolePermissions(string roleName) => $"keycloak:role:{roleName}:permissions";
    }
}
