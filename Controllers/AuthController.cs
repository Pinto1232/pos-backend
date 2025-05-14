using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IHttpClientFactory httpClientFactory, ILogger<AuthController> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(15); // Increase timeout to 15 seconds
        _logger = logger;
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
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
