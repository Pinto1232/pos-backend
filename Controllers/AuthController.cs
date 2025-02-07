using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IHttpClientFactory httpClientFactory, ILogger<AuthController> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
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

            var request = new HttpRequestMessage(HttpMethod.Post, "http://keycloak:8282/realms/pisval-pos-realm/protocol/openid-connect/token")

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
