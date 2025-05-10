using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using PosBackend.Application.Interfaces;
using PosBackend.Models;
using PosBackend.Utilities;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserSyncController : ControllerBase
    {
        private readonly ILogger<UserSyncController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IKeycloakService _keycloakService;
        private readonly IRoleMappingService _roleMappingService;

        public UserSyncController(
            ILogger<UserSyncController> logger,
            IUserRepository userRepository,
            IKeycloakService keycloakService,
            IRoleMappingService roleMappingService)
        {
            _logger = logger;
            _userRepository = userRepository;
            _keycloakService = keycloakService;
            _roleMappingService = roleMappingService;
        }

        // POST: api/UserSync/sync-all
        [HttpPost("sync-all")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> SyncAllUsers()
        {
            try
            {
                _logger.LogInformation("Starting synchronization of all Keycloak users");

                // Get all users from Keycloak
                var keycloakUsers = await _keycloakService.GetAllUsersAsync();
                if (keycloakUsers == null || !keycloakUsers.Any())
                {
                    return Ok(new { message = "No users found in Keycloak" });
                }

                int syncedCount = 0;
                List<string> errors = new List<string>();

                // Sync each user
                foreach (var keycloakUser in keycloakUsers)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(keycloakUser.Email))
                        {
                            errors.Add($"User {keycloakUser.Id} has no email");
                            continue;
                        }

                        // Check if user exists in our database
                        var appUser = await _userRepository.GetByEmailAsync(keycloakUser.Email);

                        if (appUser == null)
                        {
                            // Create new user in our database
                            var newUser = new User
                            {
                                UserName = keycloakUser.Username ?? keycloakUser.Email,
                                Email = keycloakUser.Email,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                SecurityStamp = keycloakUser.Id // Store Keycloak ID in SecurityStamp
                            };

                            appUser = await _userRepository.CreateUserAsync(newUser);
                            _logger.LogInformation($"Created new user {keycloakUser.Email} in application database");
                        }
                        else if (string.IsNullOrEmpty(appUser.SecurityStamp))
                        {
                            // Update the SecurityStamp with the Keycloak user ID if it's not set
                            appUser.SecurityStamp = keycloakUser.Id;
                            await _userRepository.UpdateUserAsync(appUser);
                            _logger.LogInformation($"Updated SecurityStamp for user {keycloakUser.Email}");
                        }

                        // Sync roles for this user
                        if (appUser != null && !string.IsNullOrEmpty(keycloakUser.Id))
                        {
                            await _roleMappingService.SynchronizeUserRolesAsync(keycloakUser.Id, appUser.UserId);
                            syncedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error syncing user {keycloakUser.Email ?? keycloakUser.Id}");
                        errors.Add($"Error syncing user {keycloakUser.Email ?? keycloakUser.Id}: {ex.Message}");
                    }
                }

                return Ok(new
                {
                    message = $"Synchronized {syncedCount} users from Keycloak",
                    syncedCount,
                    totalCount = keycloakUsers.Count,
                    errors = errors.Any() ? errors : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing users from Keycloak");
                return StatusCode(500, new { error = "Failed to synchronize users", message = ex.Message });
            }
        }

        // POST: api/UserSync/update-login-status
        [HttpPost("update-login-status")]
        public async Task<IActionResult> UpdateLoginStatus()
        {
            try
            {
                _logger.LogInformation("UpdateLoginStatus endpoint called");

                // Extract token from request
                var token = JwtTokenUtils.ExtractTokenFromRequest(Request);
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No token found in request");
                    return BadRequest(new { error = "No token found in request" });
                }

                _logger.LogInformation($"Token found: {token.Substring(0, Math.Min(20, token.Length))}...");

                // Extract user info from token
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jsonToken == null)
                {
                    _logger.LogWarning("Invalid token format");
                    return BadRequest(new { error = "Invalid token format" });
                }

                // Log all claims for debugging
                _logger.LogInformation("Token claims:");
                foreach (var claim in jsonToken.Claims)
                {
                    _logger.LogInformation($"  {claim.Type}: {claim.Value}");
                }

                // Extract email from token
                var emailClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "email");
                if (emailClaim == null || string.IsNullOrEmpty(emailClaim.Value))
                {
                    _logger.LogWarning("Email not found in token");

                    // Try to find preferred_username as fallback
                    var usernameClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "preferred_username");
                    if (usernameClaim != null && !string.IsNullOrEmpty(usernameClaim.Value))
                    {
                        _logger.LogInformation($"Using preferred_username instead: {usernameClaim.Value}");
                        emailClaim = usernameClaim;
                    }
                    else
                    {
                        return BadRequest(new { error = "Email not found in token" });
                    }
                }

                var email = emailClaim.Value;
                _logger.LogInformation($"Found email in token: {email}");

                // Update the LastLogin field for the user
                _logger.LogInformation($"Looking for user with email: {email}");
                var user = await _userRepository.GetByEmailAsync(email);
                if (user != null)
                {
                    _logger.LogInformation($"Found user in database: {user.UserId}, Username: {user.UserName}, Current LastLogin: {user.LastLogin}");

                    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                    await _userRepository.LogUserLoginAttemptAsync(email, true, ipAddress);
                    _logger.LogInformation($"Logged login attempt for user: {email}");

                    var oldLastLogin = user.LastLogin;
                    user.LastLogin = DateTime.UtcNow;
                    _logger.LogInformation($"Updating LastLogin from {oldLastLogin} to {user.LastLogin}");

                    var updateResult = await _userRepository.UpdateUserAsync(user);

                    if (updateResult)
                    {
                        _logger.LogInformation($"Successfully updated LastLogin for user: {email} to {user.LastLogin}");

                        // Double-check that the update was successful by retrieving the user again
                        var updatedUser = await _userRepository.GetByEmailAsync(email);
                        if (updatedUser != null)
                        {
                            _logger.LogInformation($"Verified LastLogin update: {updatedUser.LastLogin}");

                            // Check if the LastLogin field was actually updated
                            if (updatedUser.LastLogin.HasValue &&
                                (DateTime.UtcNow - updatedUser.LastLogin.Value).TotalMinutes < 1)
                            {
                                _logger.LogInformation($"LastLogin was successfully updated to a recent time");
                            }
                            else
                            {
                                _logger.LogWarning($"LastLogin may not have been properly updated: {updatedUser.LastLogin}");

                                // Try to update directly using the DbContext
                                try
                                {
                                    updatedUser.LastLogin = DateTime.UtcNow;
                                    var directUpdateResult = await _userRepository.UpdateUserAsync(updatedUser);
                                    _logger.LogInformation($"Direct update result: {directUpdateResult}");

                                    // Verify again
                                    var finalUser = await _userRepository.GetByEmailAsync(email);
                                    if (finalUser != null)
                                    {
                                        _logger.LogInformation($"Final LastLogin value: {finalUser.LastLogin}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error during direct LastLogin update");
                                }
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Could not verify LastLogin update - user not found after update");
                        }

                        // Create a response with both ISO string and formatted timestamp
                        var response = new
                        {
                            message = $"Updated login status for {email}",
                            timestamp = user.LastLogin,
                            timestampIso = user.LastLogin?.ToString("o"),
                            timestampFormatted = user.LastLogin?.ToString("yyyy-MM-dd HH:mm:ss")
                        };

                        // Log the response
                        _logger.LogInformation($"Returning response: {JsonSerializer.Serialize(response)}");

                        return Ok(response);
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to update LastLogin for user: {email}");
                        return StatusCode(500, new { error = "Failed to update user", message = "Database update failed" });
                    }
                }
                else
                {
                    _logger.LogWarning($"User with email {email} not found in database");

                    // Try to extract Keycloak ID and create user
                    var subClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub");
                    if (subClaim != null && !string.IsNullOrEmpty(subClaim.Value))
                    {
                        _logger.LogInformation($"Creating new user for Keycloak ID: {subClaim.Value}");

                        var newUser = new User
                        {
                            UserName = email,
                            Email = email,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            LastLogin = DateTime.UtcNow,
                            SecurityStamp = subClaim.Value // Store Keycloak ID in SecurityStamp
                        };

                        var createdUser = await _userRepository.CreateUserAsync(newUser);
                        if (createdUser != null)
                        {
                            _logger.LogInformation($"Created new user: {createdUser.UserId}, Username: {createdUser.UserName}");
                            return Ok(new { message = $"Created and updated login status for {email}", timestamp = createdUser.LastLogin });
                        }
                        else
                        {
                            _logger.LogWarning("Failed to create new user");
                            return StatusCode(500, new { error = "Failed to create user", message = "Database insert failed" });
                        }
                    }

                    return NotFound(new { error = $"User with email {email} not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating login status");
                return StatusCode(500, new { error = "Failed to update login status", message = ex.Message });
            }
        }
    }
}
