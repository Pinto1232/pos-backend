using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace PosBackend.Security
{
    /// <summary>
    /// API Key authentication handler for system-level access
    /// </summary>
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiKeyAuthenticationHandler> _logger;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IConfiguration configuration)
            : base(options, logger, encoder)
        {
            _configuration = configuration;
            _logger = logger.CreateLogger<ApiKeyAuthenticationHandler>();
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Check if API key is present in headers
            if (!Request.Headers.TryGetValue(SecurityConstants.ApiKeys.HeaderName, out var apiKeyHeaderValues))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
            
            if (string.IsNullOrWhiteSpace(providedApiKey))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            // Get valid API keys from configuration
            var systemApiKey = _configuration[SecurityConstants.ApiKeys.SystemApiKey] 
                             ?? Environment.GetEnvironmentVariable(SecurityConstants.ApiKeys.SystemApiKey);

            if (string.IsNullOrWhiteSpace(systemApiKey))
            {
                _logger.LogWarning("System API key not configured");
                return Task.FromResult(AuthenticateResult.Fail("API key authentication not configured"));
            }

            // Validate API key
            if (providedApiKey != systemApiKey)
            {
                _logger.LogWarning("Invalid API key provided from IP: {RemoteIpAddress}", 
                    Context.Connection.RemoteIpAddress);
                return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));
            }

            // Create claims for the system user
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "System"),
                new Claim(SecurityConstants.Claims.Role, SecurityConstants.Roles.SystemAdmin),
                new Claim(ClaimTypes.AuthenticationMethod, "ApiKey")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            _logger.LogInformation("API key authentication successful");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public const string Scheme = "ApiKey";
    }
}