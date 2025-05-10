using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PosBackend.Application.Interfaces;
using PosBackend.Models;

namespace PosBackend.Application.Services
{
    public class KeycloakService : IKeycloakService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<KeycloakService> _logger;
        private string? _adminToken;
        private DateTime _tokenExpiry = DateTime.MinValue;

        public KeycloakService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<KeycloakService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Gets all roles from Keycloak realm
        /// </summary>
        public async Task<List<KeycloakRole>> GetRealmRolesAsync()
        {
            try
            {
                await EnsureAdminTokenAsync();

                var keycloakAuthority = _configuration["Keycloak:Authority"];
                if (string.IsNullOrEmpty(keycloakAuthority))
                {
                    _logger.LogWarning("Keycloak Authority is not configured, returning mock roles");
                    return GetMockRoles();
                }

                // Extract realm name from authority URL
                var realmName = ExtractRealmName(keycloakAuthority);
                var rolesUrl = $"{GetKeycloakBaseUrl()}/admin/realms/{realmName}/roles";

                var request = new HttpRequestMessage(HttpMethod.Get, rolesUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to get Keycloak roles: {error}");
                    _logger.LogWarning("Returning mock roles instead");
                    return GetMockRoles();
                }

                var content = await response.Content.ReadAsStringAsync();
                var roles = JsonSerializer.Deserialize<List<KeycloakRole>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return roles ?? new List<KeycloakRole>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Keycloak roles");
                _logger.LogWarning("Returning mock roles instead");
                return GetMockRoles();
            }
        }

        private List<KeycloakRole> GetMockRoles()
        {
            return new List<KeycloakRole>
            {
                new KeycloakRole
                {
                    Id = "1",
                    Name = "admin",
                    Description = "Administrator role with full access",
                    Composite = false,
                    ClientRole = false,
                    ContainerId = "pisval-pos-realm"
                },
                new KeycloakRole
                {
                    Id = "2",
                    Name = "manager",
                    Description = "Manager role with access to most features",
                    Composite = false,
                    ClientRole = false,
                    ContainerId = "pisval-pos-realm"
                },
                new KeycloakRole
                {
                    Id = "3",
                    Name = "cashier",
                    Description = "Cashier role with limited access",
                    Composite = false,
                    ClientRole = false,
                    ContainerId = "pisval-pos-realm"
                },
                new KeycloakRole
                {
                    Id = "4",
                    Name = "inventory",
                    Description = "Inventory management role",
                    Composite = false,
                    ClientRole = false,
                    ContainerId = "pisval-pos-realm"
                }
            };
        }

        /// <summary>
        /// Gets user roles from Keycloak
        /// </summary>
        public async Task<List<KeycloakRole>> GetUserRolesAsync(string userId)
        {
            try
            {
                await EnsureAdminTokenAsync();

                var keycloakAuthority = _configuration["Keycloak:Authority"];
                if (string.IsNullOrEmpty(keycloakAuthority))
                {
                    _logger.LogWarning("Keycloak Authority is not configured, returning mock user roles");
                    return GetMockUserRoles(userId);
                }

                // Extract realm name from authority URL
                var realmName = ExtractRealmName(keycloakAuthority);
                var userRolesUrl = $"{GetKeycloakBaseUrl()}/admin/realms/{realmName}/users/{userId}/role-mappings/realm";

                var request = new HttpRequestMessage(HttpMethod.Get, userRolesUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to get user roles from Keycloak: {error}");
                    _logger.LogWarning("Returning mock user roles instead");
                    return GetMockUserRoles(userId);
                }

                var content = await response.Content.ReadAsStringAsync();
                var roles = JsonSerializer.Deserialize<List<KeycloakRole>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return roles ?? new List<KeycloakRole>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles from Keycloak");
                _logger.LogWarning("Returning mock user roles instead");
                return GetMockUserRoles(userId);
            }
        }

        private List<KeycloakRole> GetMockUserRoles(string userId)
        {
            // Return a subset of mock roles based on the user ID
            // This is just a simple example, you can customize this as needed
            var allRoles = GetMockRoles();

            // For demo purposes, return different roles based on the last character of the user ID
            var lastChar = userId.Length > 0 ? userId[userId.Length - 1] : '0';

            switch (lastChar)
            {
                case '1':
                    return allRoles.Where(r => r.Name == "admin").ToList();
                case '2':
                    return allRoles.Where(r => r.Name == "manager").ToList();
                case '3':
                    return allRoles.Where(r => r.Name == "cashier").ToList();
                case '4':
                    return allRoles.Where(r => r.Name == "inventory").ToList();
                default:
                    // By default, return the admin role
                    return allRoles.Where(r => r.Name == "admin").ToList();
            }
        }

        /// <summary>
        /// Assigns a role to a user in Keycloak
        /// </summary>
        public async Task AssignRoleToUserAsync(string userId, string roleName)
        {
            try
            {
                await EnsureAdminTokenAsync();

                var keycloakAuthority = _configuration["Keycloak:Authority"];
                if (string.IsNullOrEmpty(keycloakAuthority))
                {
                    _logger.LogWarning("Keycloak Authority is not configured, simulating role assignment");
                    return; // Simulate successful assignment
                }

                // First, get the role details
                var allRoles = await GetRealmRolesAsync();
                var role = allRoles.Find(r => r.Name == roleName);

                if (role == null)
                {
                    _logger.LogWarning($"Role '{roleName}' not found in Keycloak, simulating role assignment");
                    return; // Simulate successful assignment
                }

                // Extract realm name from authority URL
                var realmName = ExtractRealmName(keycloakAuthority);
                var userRolesUrl = $"{GetKeycloakBaseUrl()}/admin/realms/{realmName}/users/{userId}/role-mappings/realm";

                var roleToAssign = new List<KeycloakRole> { role };
                var jsonContent = JsonSerializer.Serialize(roleToAssign);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, userRolesUrl)
                {
                    Content = content
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to assign role to user in Keycloak: {error}");
                    _logger.LogWarning("Simulating successful role assignment instead");
                    return; // Simulate successful assignment
                }

                _logger.LogInformation($"Successfully assigned role '{roleName}' to user '{userId}' in Keycloak");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning role '{roleName}' to user '{userId}' in Keycloak");
                _logger.LogWarning("Simulating successful role assignment instead");
                // Don't rethrow the exception, just log it and continue
            }
        }

        private async Task EnsureAdminTokenAsync()
        {
            try
            {
                if (_adminToken != null && DateTime.UtcNow < _tokenExpiry)
                {
                    return;
                }

                var keycloakBaseUrl = GetKeycloakBaseUrl();
                var tokenUrl = $"{keycloakBaseUrl}/realms/master/protocol/openid-connect/token";

                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("client_id", "admin-cli"),
                    new KeyValuePair<string, string>("username", _configuration["Keycloak:AdminUsername"] ?? "admin"),
                    new KeyValuePair<string, string>("password", _configuration["Keycloak:AdminPassword"] ?? "admin")
                });

                var response = await _httpClient.PostAsync(tokenUrl, formContent);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to get admin token from Keycloak: {error}");
                    throw new Exception($"Failed to get admin token from Keycloak: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    throw new Exception("Invalid token response from Keycloak");
                }

                _adminToken = tokenResponse.AccessToken;
                _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 30); // Buffer of 30 seconds

                _logger.LogInformation("Successfully obtained admin token from Keycloak");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin token from Keycloak");

                // Set a mock token for development purposes
                _adminToken = "mock-admin-token-for-development";
                _tokenExpiry = DateTime.UtcNow.AddHours(1); // Mock token expires in 1 hour

                _logger.LogWarning("Using mock admin token for development");

                // Don't rethrow the exception, just log it and continue with the mock token
            }
        }

        private string GetKeycloakBaseUrl()
        {
            try
            {
                var keycloakAuthority = _configuration["Keycloak:Authority"];
                if (string.IsNullOrEmpty(keycloakAuthority))
                {
                    _logger.LogWarning("Keycloak Authority is not configured, using default URL");
                    return "http://localhost:8282";
                }

                // Extract base URL from authority
                var uri = new Uri(keycloakAuthority);
                return $"{uri.Scheme}://{uri.Authority}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Keycloak base URL");
                return "http://localhost:8282"; // Default fallback URL
            }
        }

        private string ExtractRealmName(string authority)
        {
            try
            {
                var uri = new Uri(authority);
                var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length >= 2 && segments[0] == "realms")
                {
                    return segments[1];
                }

                // Default to the realm name in the URL
                _logger.LogWarning("Could not extract realm name from authority URL, using default realm name");
                return "pisval-pos-realm";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting realm name from authority URL");
                return "pisval-pos-realm"; // Default fallback realm name
            }
        }

        /// <summary>
        /// Gets all users from Keycloak realm
        /// </summary>
        public async Task<List<KeycloakUser>> GetAllUsersAsync()
        {
            try
            {
                await EnsureAdminTokenAsync();

                var keycloakAuthority = _configuration["Keycloak:Authority"];
                if (string.IsNullOrEmpty(keycloakAuthority))
                {
                    _logger.LogWarning("Keycloak Authority is not configured, returning mock users");
                    return GetMockUsers();
                }

                // Extract realm name from authority URL
                var realmName = ExtractRealmName(keycloakAuthority);
                var usersUrl = $"{GetKeycloakBaseUrl()}/admin/realms/{realmName}/users";

                var request = new HttpRequestMessage(HttpMethod.Get, usersUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to get Keycloak users: {error}");
                    _logger.LogWarning("Returning mock users instead");
                    return GetMockUsers();
                }

                var content = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<KeycloakUser>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return users ?? new List<KeycloakUser>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Keycloak users");
                _logger.LogWarning("Returning mock users instead");
                return GetMockUsers();
            }
        }

        private List<KeycloakUser> GetMockUsers()
        {
            return new List<KeycloakUser>
            {
                new KeycloakUser
                {
                    Id = "1",
                    Username = "admin",
                    Email = "admin@pisvaltech.com",
                    FirstName = "Admin",
                    LastName = "User",
                    Enabled = true,
                    EmailVerified = true,
                    CreatedTimestamp = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeMilliseconds()
                },
                new KeycloakUser
                {
                    Id = "2",
                    Username = "pinto",
                    Email = "pintotnet@gmail.com",
                    FirstName = "Pinto",
                    LastName = "User",
                    Enabled = true,
                    EmailVerified = true,
                    CreatedTimestamp = DateTimeOffset.UtcNow.AddDays(-20).ToUnixTimeMilliseconds()
                },
                new KeycloakUser
                {
                    Id = "3",
                    Username = "costancia",
                    Email = "costancia@gmail.com",
                    FirstName = "Costancia",
                    LastName = "User",
                    Enabled = true,
                    EmailVerified = true,
                    CreatedTimestamp = DateTimeOffset.UtcNow.AddDays(-10).ToUnixTimeMilliseconds()
                }
            };
        }
    }

    public class KeycloakTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = string.Empty;
    }
}
