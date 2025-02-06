using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public AuthController(IConfiguration config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(refreshToken)) return Unauthorized();

        var keycloakUrl = $"{_config["Keycloak:Authority"]}/protocol/openid-connect/token";

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "client_id", _config["Keycloak:ClientId"] },
            { "client_secret", _config["Keycloak:ClientSecret"] },
            { "refresh_token", refreshToken }
        });

        var response = await _httpClient.PostAsync(keycloakUrl, content);
        if (!response.IsSuccessStatusCode) return Unauthorized();

        var tokenResponse = await response.Content.ReadAsStringAsync();
        return Ok(tokenResponse);
    }
}
