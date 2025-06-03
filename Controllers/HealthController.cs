using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosBackend.Services;
using System;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly SubscriptionService _subscriptionService;

        public HealthController(
            ILogger<HealthController> logger,
            SubscriptionService subscriptionService)
        {
            _logger = logger;
            _subscriptionService = subscriptionService;
        }

        /// <summary>
        /// Health check endpoint to verify the API is running
        /// </summary>
        /// <returns>Health status information</returns>
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Health check endpoint called at {time}", DateTime.UtcNow);
            
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
            });
        }

        /// <summary>
        /// Test subscription service health for a specific user
        /// </summary>
        /// <param name="userId">User ID to test</param>
        /// <returns>Subscription service health status</returns>
        [HttpGet("subscription/{userId}")]
        public async Task<IActionResult> GetSubscriptionHealth(string userId)
        {
            try
            {
                _logger.LogInformation("Testing subscription service for user {UserId}", userId);

                var details = await _subscriptionService.GetSubscriptionDetailsAsync(userId);
                var history = await _subscriptionService.GetBillingHistoryAsync(userId, 1);

                return Ok(new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    userId = userId,
                    hasSubscriptionDetails = details != null,
                    billingHistoryCount = history?.Count ?? 0,
                    subscriptionService = "working"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Subscription service test failed for user {UserId}", userId);
                return StatusCode(500, new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    userId = userId,
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
