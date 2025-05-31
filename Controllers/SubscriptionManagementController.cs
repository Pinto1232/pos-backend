using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PosBackend.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionManagementController : ControllerBase
    {
        private readonly SubscriptionService _subscriptionService;
        private readonly ILogger<SubscriptionManagementController> _logger;
        private const string InternalServerErrorMessage = "An internal server error occurred. Please try again later.";

        public SubscriptionManagementController(
            SubscriptionService subscriptionService,
            ILogger<SubscriptionManagementController> logger)
        {
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        // GET: api/SubscriptionManagement/user/{userId}/details
        [HttpGet("user/{userId}/details")]
        public async Task<ActionResult<object>> GetSubscriptionDetails(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var details = await _subscriptionService.GetSubscriptionDetailsAsync(userId);
                if (details == null)
                {
                    return NotFound(new { error = "No active subscription found for this user" });
                }

                return Ok(details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription details for user {UserId}", userId);
                return StatusCode(500, new { error = InternalServerErrorMessage });
            }
        }

        // GET: api/SubscriptionManagement/user/{userId}/billing-history
        [HttpGet("user/{userId}/billing-history")]
        public async Task<ActionResult<List<object>>> GetBillingHistory(string userId, [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                if (limit <= 0 || limit > 100)
                {
                    limit = 10;
                }

                var history = await _subscriptionService.GetBillingHistoryAsync(userId, limit);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing history for user {UserId}", userId);
                return StatusCode(500, new { error = InternalServerErrorMessage });
            }
        }

        // POST: api/SubscriptionManagement/change-plan
        [HttpPost("change-plan")]
        public async Task<ActionResult<object>> ChangePlan([FromBody] ChangePlanRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserId) || request.NewPackageId <= 0)
                {
                    return BadRequest(new { error = "UserId and NewPackageId are required" });
                }

                var updatedSubscription = await _subscriptionService.ChangeSubscriptionPlanAsync(
                    request.UserId,
                    request.NewPackageId,
                    request.Prorated);

                if (updatedSubscription == null)
                {
                    return BadRequest(new { error = "Failed to change subscription plan" });
                }

                return Ok(new { message = "Subscription plan changed successfully", subscription = updatedSubscription });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing plan for user {UserId}", request.UserId);
                return StatusCode(500, new { error = InternalServerErrorMessage });
            }
        }

        // POST: api/SubscriptionManagement/update-payment-method
        [HttpPost("update-payment-method")]
        public async Task<ActionResult> UpdatePaymentMethod([FromBody] UpdatePaymentMethodRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.PaymentMethodId))
                {
                    return BadRequest(new { error = "UserId and PaymentMethodId are required" });
                }

                var success = await _subscriptionService.UpdatePaymentMethodAsync(request.UserId, request.PaymentMethodId);
                if (!success)
                {
                    return BadRequest(new { error = "Failed to update payment method" });
                }

                return Ok(new { message = "Payment method updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment method for user {UserId}", request.UserId);
                return StatusCode(500, new { error = InternalServerErrorMessage });
            }
        }

        // POST: api/SubscriptionManagement/pause
        [HttpPost("pause")]
        public async Task<ActionResult> PauseSubscription([FromBody] SubscriptionActionRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var success = await _subscriptionService.PauseSubscriptionAsync(request.UserId);
                if (!success)
                {
                    return BadRequest(new { error = "Failed to pause subscription" });
                }

                return Ok(new { message = "Subscription paused successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing subscription for user {UserId}", request.UserId);
                return StatusCode(500, new { error = InternalServerErrorMessage });
            }
        }

        // POST: api/SubscriptionManagement/resume
        [HttpPost("resume")]
        public async Task<ActionResult> ResumeSubscription([FromBody] SubscriptionActionRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var success = await _subscriptionService.ResumeSubscriptionAsync(request.UserId);
                if (!success)
                {
                    return BadRequest(new { error = "Failed to resume subscription" });
                }

                return Ok(new { message = "Subscription resumed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming subscription for user {UserId}", request.UserId);
                return StatusCode(500, new { error = InternalServerErrorMessage });
            }
        }

        // POST: api/SubscriptionManagement/reactivate
        [HttpPost("reactivate")]
        public async Task<ActionResult> ReactivateSubscription([FromBody] SubscriptionActionRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var success = await _subscriptionService.ReactivateSubscriptionAsync(request.UserId);
                if (!success)
                {
                    return BadRequest(new { error = "Failed to reactivate subscription" });
                }

                return Ok(new { message = "Subscription reactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating subscription for user {UserId}", request.UserId);
                return StatusCode(500, new { error = InternalServerErrorMessage });
            }
        }
    }

    // DTOs
    public class ChangePlanRequest
    {
        public string UserId { get; set; } = string.Empty;
        public int NewPackageId { get; set; }
        public bool Prorated { get; set; } = true;
    }

    public class UpdatePaymentMethodRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string PaymentMethodId { get; set; } = string.Empty;
    }


}
