using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using Microsoft.Extensions.Configuration;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;

    public AuthController(IHttpClientFactory httpClientFactory, ILogger<AuthController> logger, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(15); // Increase timeout to 15 seconds
        _logger = logger;
        _configuration = configuration;
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

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(10));

            HttpResponseMessage response;
            string content;

            try
            {
                _logger.LogInformation("Sending request to Keycloak...");
                response = await _httpClient.SendAsync(loginRequest, cts.Token);
                content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"⚠️ Login failed: {response.StatusCode}, Response: {content}");
                    return StatusCode((int)response.StatusCode, new { error = "Login failed", message = "Authentication failed. Please check your credentials." });
                }
            }
            catch (TaskCanceledException ex) when (cts.IsCancellationRequested)
            {
                _logger.LogError($"⏱️ Keycloak request timed out: {ex.Message}");
                return StatusCode(504, new { error = "Gateway Timeout", message = "Authentication server is not responding. Please try again later." });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"🌐 Network error connecting to Keycloak: {ex.Message}");
                return StatusCode(502, new { error = "Bad Gateway", message = "Unable to connect to authentication server. Please try again later." });
            }

            var jsonResponse = JsonSerializer.Deserialize<object>(content);
            _logger.LogInformation("✅ Login successful.");
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

    [HttpPost("keycloak/callback")]
    public async Task<IActionResult> KeycloakCallback([FromBody] CallbackRequest request)
    {
        var requestStartTime = DateTime.UtcNow;
        var requestId = Guid.NewGuid().ToString().Substring(0, 8);

        _logger.LogInformation($"[{requestId}] [START] 🔄 Keycloak callback received at {requestStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}");

        try
        {
            _logger.LogInformation($"[{requestId}] [CODE] Authorization code received, length: {request.Code?.Length ?? 0}");

            if (string.IsNullOrEmpty(request.Code))
            {
                _logger.LogWarning($"[{requestId}] [ERROR] ⚠️ No authorization code provided in callback");
                return BadRequest(new { error = "Invalid request", message = "Authorization code is missing" });
            }

            // Get Keycloak configuration from appsettings.json
            string keycloakAuthority = _configuration["Keycloak:Authority"] ?? "http://localhost:8282/realms/pisval-pos-realm";
            string clientId = _configuration["Keycloak:ClientId"] ?? "pos-backend";
            string clientSecret = _configuration["Keycloak:ClientSecret"] ?? "mKvMzX6Klgc4yMFRmqs3H3OtSRwa0B3b";
            string redirectUri = "http://localhost:3000/after-auth"; // This should match the frontend redirect URI

            _logger.LogInformation($"[{requestId}] [CONFIG] Using Keycloak config: Authority={keycloakAuthority}, ClientId={clientId}, RedirectUri={redirectUri}");

            // Construct token endpoint URL
            string tokenEndpoint = $"{keycloakAuthority.TrimEnd('/')}/protocol/openid-connect/token";
            _logger.LogInformation($"[{requestId}] [ENDPOINT] Token endpoint: {tokenEndpoint}");

            // Exchange the authorization code for tokens
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", request.Code),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri)
                })
            };

            tokenRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _logger.LogInformation($"[{requestId}] [REQUEST] Prepared token request with headers: {string.Join(", ", tokenRequest.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(10));

            _logger.LogInformation($"[{requestId}] [REQUEST] Sending token request to Keycloak at {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")}...");

            try
            {
                var requestSentTime = DateTime.UtcNow;
                var response = await _httpClient.SendAsync(tokenRequest, cts.Token);
                var responseReceivedTime = DateTime.UtcNow;
                var requestDuration = responseReceivedTime - requestSentTime;

                _logger.LogInformation($"[{requestId}] [RESPONSE] Received response from Keycloak after {requestDuration.TotalMilliseconds}ms with status code: {(int)response.StatusCode} {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"[{requestId}] [RESPONSE] Response content length: {content?.Length ?? 0} bytes");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"[{requestId}] [ERROR] ⚠️ Code exchange failed: {response.StatusCode}, Response: {content}");
                    return StatusCode((int)response.StatusCode, new { error = "Token exchange failed", message = "Failed to exchange authorization code for tokens." });
                }

                // Parse the token response
                _logger.LogInformation($"[{requestId}] [PARSING] Parsing token response...");
                var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content ?? "{}");

                // Check if required properties exist
                bool hasAccessToken = tokenResponse.TryGetProperty("access_token", out var _);
                bool hasRefreshToken = tokenResponse.TryGetProperty("refresh_token", out var _);
                bool hasExpiresIn = tokenResponse.TryGetProperty("expires_in", out var _);
                bool hasTokenType = tokenResponse.TryGetProperty("token_type", out var _);

                _logger.LogInformation($"[{requestId}] [VALIDATION] Token response validation: AccessToken={hasAccessToken}, RefreshToken={hasRefreshToken}, ExpiresIn={hasExpiresIn}, TokenType={hasTokenType}");

                if (!hasAccessToken || !hasRefreshToken || !hasExpiresIn || !hasTokenType)
                {
                    _logger.LogWarning($"[{requestId}] [ERROR] ⚠️ Incomplete token response from Keycloak");
                    return StatusCode(502, new { error = "Incomplete Response", message = "Received incomplete token response from authentication server." });
                }

                // Extract token details for logging (safely)
                string accessToken = tokenResponse.GetProperty("access_token").GetString() ?? "";
                string refreshToken = tokenResponse.GetProperty("refresh_token").GetString() ?? "";
                int expiresIn = tokenResponse.GetProperty("expires_in").GetInt32();
                string tokenType = tokenResponse.GetProperty("token_type").GetString() ?? "";

                _logger.LogInformation($"[{requestId}] [SUCCESS] ✅ Authorization code exchange successful. Access token length: {accessToken.Length}, Refresh token length: {refreshToken.Length}, Expires in: {expiresIn}s, Token type: {tokenType}");

                var totalDuration = DateTime.UtcNow - requestStartTime;
                _logger.LogInformation($"[{requestId}] [COMPLETE] Total request processing time: {totalDuration.TotalMilliseconds}ms");

                // Return the tokens to the client
                return Ok(new
                {
                    accessToken,
                    refreshToken,
                    expiresIn,
                    tokenType
                });
            }
            catch (TaskCanceledException ex) when (cts.IsCancellationRequested)
            {
                _logger.LogError($"[{requestId}] [TIMEOUT] ⏱️ Keycloak token request timed out after 10 seconds: {ex.Message}");
                return StatusCode(504, new { error = "Gateway Timeout", message = "Authentication server is not responding. Please try again later." });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"[{requestId}] [NETWORK_ERROR] 🌐 Network error connecting to Keycloak: {ex.Message}");
                return StatusCode(502, new { error = "Bad Gateway", message = "Unable to connect to authentication server. Please try again later." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"[{requestId}] [EXCEPTION] ❌ Unhandled exception during code exchange: {ex.Message}");
            _logger.LogError($"[{requestId}] [EXCEPTION_DETAILS] Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
        finally
        {
            var totalProcessingTime = DateTime.UtcNow - requestStartTime;
            _logger.LogInformation($"[{requestId}] [END] Keycloak callback processing completed in {totalProcessingTime.TotalMilliseconds}ms");
        }
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class CallbackRequest
{
    public string Code { get; set; } = string.Empty;
}
