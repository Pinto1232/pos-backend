using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Text.Json;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TestController> _logger;

        public TestController(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<TestController> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "API is working correctly!" });
        }

        [HttpGet("keycloak-config")]
        public async Task<IActionResult> GetKeycloakConfig()
        {
            try
            {
                // Get Keycloak configuration from appsettings.json
                string keycloakAuthority = _configuration["Keycloak:Authority"] ?? "http://localhost:8282/realms/pisval-pos-realm";
                string clientId = _configuration["Keycloak:ClientId"] ?? "pos-backend";
                string clientSecret = _configuration["Keycloak:ClientSecret"] ?? "";

                // Create a sanitized version of the configuration (without sensitive data)
                var configInfo = new
                {
                    Authority = keycloakAuthority,
                    ClientId = clientId,
                    HasClientSecret = !string.IsNullOrEmpty(clientSecret),
                    WellKnownEndpoint = $"{keycloakAuthority}/.well-known/openid-configuration"
                };

                // Check if Keycloak is reachable
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);

                bool isKeycloakReachable = false;
                string? wellKnownConfigJson = null;

                try
                {
                    _logger.LogInformation("Testing connection to Keycloak well-known endpoint");
                    var response = await httpClient.GetAsync($"{keycloakAuthority}/.well-known/openid-configuration");

                    if (response.IsSuccessStatusCode)
                    {
                        isKeycloakReachable = true;
                        wellKnownConfigJson = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation("Successfully connected to Keycloak");
                    }
                    else
                    {
                        _logger.LogWarning($"Keycloak returned non-success status code: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error connecting to Keycloak: {ex.Message}");
                }

                // Parse the well-known configuration if available
                JsonElement? wellKnownConfig = null;
                if (!string.IsNullOrEmpty(wellKnownConfigJson))
                {
                    try
                    {
                        wellKnownConfig = JsonSerializer.Deserialize<JsonElement>(wellKnownConfigJson);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error parsing Keycloak well-known configuration: {ex.Message}");
                    }
                }

                // Extract important endpoints from the well-known configuration
                string? authorizationEndpoint = null;
                string? tokenEndpoint = null;
                string[]? supportedResponseTypes = null;
                string[]? supportedGrantTypes = null;
                string[]? supportedPkceCodeChallengeMethods = null;

                if (wellKnownConfig.HasValue)
                {
                    var config = wellKnownConfig.Value;

                    if (config.TryGetProperty("authorization_endpoint", out var authEndpoint))
                    {
                        authorizationEndpoint = authEndpoint.GetString();
                    }

                    if (config.TryGetProperty("token_endpoint", out var tokenEndp))
                    {
                        tokenEndpoint = tokenEndp.GetString();
                    }

                    if (config.TryGetProperty("response_types_supported", out var responseTypes))
                    {
                        supportedResponseTypes = JsonSerializer.Deserialize<string[]>(responseTypes.GetRawText()) ?? Array.Empty<string>();
                    }

                    if (config.TryGetProperty("grant_types_supported", out var grantTypes))
                    {
                        supportedGrantTypes = JsonSerializer.Deserialize<string[]>(grantTypes.GetRawText()) ?? Array.Empty<string>();
                    }

                    if (config.TryGetProperty("code_challenge_methods_supported", out var pkceMethodsElement))
                    {
                        supportedPkceCodeChallengeMethods = JsonSerializer.Deserialize<string[]>(pkceMethodsElement.GetRawText()) ?? Array.Empty<string>();
                    }
                }

                return Ok(new
                {
                    Configuration = configInfo,
                    Status = new
                    {
                        IsKeycloakReachable = isKeycloakReachable,
                        AuthorizationEndpoint = authorizationEndpoint,
                        TokenEndpoint = tokenEndpoint,
                        SupportsPKCE = supportedPkceCodeChallengeMethods != null && supportedPkceCodeChallengeMethods.Contains("S256"),
                        SupportsAuthorizationCode = supportedResponseTypes != null && supportedResponseTypes.Contains("code"),
                        SupportedResponseTypes = supportedResponseTypes,
                        SupportedGrantTypes = supportedGrantTypes,
                        SupportedPKCEMethods = supportedPkceCodeChallengeMethods
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking Keycloak configuration: {ex.Message}");
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
            }
        }
    }
}
