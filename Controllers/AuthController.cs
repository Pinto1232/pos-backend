using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PosBackend.Application.Interfaces;
using PosBackend.Utilities;
using System.IdentityModel.Tokens.Jwt;
using PosBackend.Models;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthController> _logger;
    private readonly IUserRepository _userRepository;

    public AuthController(
        IHttpClientFactory httpClientFactory,
        ILogger<AuthController> logger,
        IUserRepository userRepository)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
        _userRepository = userRepository;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation($"🔐 Attempting login for user: {request.Email}");

            var loginRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8282/realms/pisval-pos-realm/protocol/openid-connect/token")
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", request.Email),
                    new KeyValuePair<string, string>("password", request.Password),
                    new KeyValuePair<string, string>("client_id", "pos-backend"),
                    new KeyValuePair<string, string>("client_secret", "mKvMzX6Klgc4yMFRmqs3H3OtSRwa0B3b")
                })
            };

            loginRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(loginRequest);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"⚠️ Login failed: {response.StatusCode}, Response: {content}");
                return StatusCode((int)response.StatusCode, new { error = "Login failed", message = content });
            }

            var jsonResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(content);
            _logger.LogInformation("✅ Login successful.");

            // Extract user information from the token
            if (jsonResponse?.AccessToken != null)
            {
                try
                {
                    // Get the user ID from the token
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadToken(jsonResponse.AccessToken) as JwtSecurityToken;

                    if (jsonToken != null)
                    {
                        // Extract email from token
                        var emailClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "email");
                        var email = emailClaim?.Value ?? request.Email;

                        // Extract Keycloak user ID from token
                        var subClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub");
                        var keycloakUserId = subClaim?.Value;

                        // Log the login attempt and update LastLogin
                        if (!string.IsNullOrEmpty(email))
                        {
                            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                            await _userRepository.LogUserLoginAttemptAsync(email, true, ipAddress);

                            // Update the LastLogin field for the user
                            var user = await _userRepository.GetByEmailAsync(email);
                            if (user != null)
                            {
                                user.LastLogin = DateTime.UtcNow;
                                await _userRepository.UpdateUserAsync(user);
                                _logger.LogInformation($"Updated LastLogin for user: {email}");
                            }
                            else if (!string.IsNullOrEmpty(keycloakUserId))
                            {
                                // User exists in Keycloak but not in our database - create a placeholder
                                _logger.LogInformation($"User {email} exists in Keycloak but not in local database. Creating placeholder.");

                                var newUser = new User
                                {
                                    UserName = email,
                                    Email = email,
                                    IsActive = true,
                                    CreatedAt = DateTime.UtcNow,
                                    LastLogin = DateTime.UtcNow,
                                    SecurityStamp = keycloakUserId // Store Keycloak ID in SecurityStamp
                                };

                                await _userRepository.CreateUserAsync(newUser);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating login history");
                    // Don't fail the login if this fails
                }
            }

            return Ok(jsonResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Exception during login: {ex.Message}");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { error = "Invalid request", message = "Token is missing" });

            _logger.LogInformation("🔄 Refresh token request received.");
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8282/realms/pisval-pos-realm/protocol/openid-connect/token")
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", token),
                    new KeyValuePair<string, string>("client_id", "pos-backend"),
                    new KeyValuePair<string, string>("client_secret", "mKvMzX6Klgc4yMFRmqs3H3OtSRwa0B3b")
                })
            };

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"⚠️ Token refresh failed: {response.StatusCode}");
                return StatusCode((int)response.StatusCode, new { error = "Token refresh failed", message = content });
            }

            var jsonResponse = JsonSerializer.Deserialize<object>(content);
            _logger.LogInformation("✅ Token refreshed successfully.");
            return Ok(jsonResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Exception during token refresh: {ex.Message}");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class KeycloakTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_expires_in")]
    public int RefreshExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("id_token")]
    public string? IdToken { get; set; }

    [JsonPropertyName("session_state")]
    public string? SessionState { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}
