using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace PosBackend.Utilities
{
    public static class JwtTokenUtils
    {
        private static readonly ILogger _logger;

        static JwtTokenUtils()
        {
            // Create a logger factory for static class
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            _logger = loggerFactory.CreateLogger("JwtTokenUtils");
        }
        /// <summary>
        /// Extracts the Keycloak roles from a JWT token
        /// </summary>
        public static List<string> ExtractRolesFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return new List<string>();
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jsonToken == null)
                {
                    return new List<string>();
                }

                // Try to get roles from the 'realm_access' claim first (Keycloak format)
                var realmAccessClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "realm_access");
                if (realmAccessClaim != null && !string.IsNullOrEmpty(realmAccessClaim.Value))
                {
                    try
                    {
                        // Parse the JSON value of the realm_access claim
                        var realmAccess = System.Text.Json.JsonSerializer.Deserialize<RealmAccess>(realmAccessClaim.Value);
                        if (realmAccess != null && realmAccess.Roles != null)
                        {
                            return realmAccess.Roles;
                        }
                    }
                    catch
                    {
                        // Ignore parsing errors and try other methods
                    }
                }

                // Try to get roles from the 'roles' claim
                var rolesClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "roles");
                if (rolesClaim != null && !string.IsNullOrEmpty(rolesClaim.Value))
                {
                    try
                    {
                        return System.Text.Json.JsonSerializer.Deserialize<List<string>>(rolesClaim.Value) ?? new List<string>();
                    }
                    catch
                    {
                        // Ignore parsing errors and try other methods
                    }
                }

                // Try to get roles from standard role claims
                var roleClaims = jsonToken.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type == "role").ToList();
                if (roleClaims.Any())
                {
                    return roleClaims.Select(c => c.Value).ToList();
                }

                return new List<string>();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Extracts the user ID from a JWT token
        /// </summary>
        public static string ExtractUserIdFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return string.Empty;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jsonToken == null)
                {
                    return string.Empty;
                }

                // Try to get the subject claim (Keycloak uses 'sub' for user ID)
                var subClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub");
                if (subClaim != null)
                {
                    return subClaim.Value;
                }

                // Try standard name identifier claim
                var nameIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (nameIdClaim != null)
                {
                    return nameIdClaim.Value;
                }

                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Extracts the token from the HTTP request
        /// </summary>
        public static string ExtractTokenFromRequest(HttpRequest request)
        {
            if (request == null)
            {
                _logger.LogWarning("Request is null in ExtractTokenFromRequest");
                return string.Empty;
            }

            _logger.LogInformation("Extracting token from request");

            // Log all headers for debugging
            _logger.LogInformation("Request headers:");
            foreach (var header in request.Headers)
            {
                _logger.LogInformation($"  {header.Key}: {(header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ? "[REDACTED]" : header.Value)}");
            }

            // Try to get the token from the Authorization header
            if (request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var authHeaderValue = authHeader.FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeaderValue))
                {
                    _logger.LogInformation("Authorization header found");

                    if (authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        var token = authHeaderValue.Substring("Bearer ".Length).Trim();
                        _logger.LogInformation($"Bearer token extracted, length: {token.Length}");
                        return token;
                    }
                    else
                    {
                        _logger.LogWarning("Authorization header does not start with 'Bearer '");
                    }
                }
                else
                {
                    _logger.LogWarning("Authorization header is empty");
                }
            }
            else
            {
                _logger.LogWarning("No Authorization header found in request");
            }

            // Try to get token from cookies as fallback
            if (request.Cookies.TryGetValue("auth_token", out var cookieToken))
            {
                _logger.LogInformation("Found token in auth_token cookie");
                return cookieToken;
            }

            // Try to get token from query string as fallback
            var queryToken = request.Query["token"].FirstOrDefault();
            if (!string.IsNullOrEmpty(queryToken))
            {
                _logger.LogInformation("Found token in query string");
                return queryToken;
            }

            _logger.LogWarning("No token found in request");
            return string.Empty;
        }

        private class RealmAccess
        {
            public List<string> Roles { get; set; } = new List<string>();
        }
    }
}
