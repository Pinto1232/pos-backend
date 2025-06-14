using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PosBackend.Models;
using PosBackend.Services;
using PosBackend.Security;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = SecurityConstants.Policies.RequireValidSubscription)]
    [EnableRateLimiting(SecurityConstants.RateLimiting.AuthenticatedUserPolicy)]
    public class SubscriptionController : SecureBaseController
    {
        private const string InternalServerErrorMessage = "Internal server error";

        private readonly SubscriptionService _subscriptionService;
        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(
            SubscriptionService subscriptionService,
            ILogger<SubscriptionController> logger)
        {
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        // POST: api/Subscription/create
        [HttpPost("create")]
        public async Task<ActionResult<SubscriptionResponse>> CreateSubscription([FromBody] CreateSubscriptionRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserId) || request.PackageId <= 0)
                {
                    return BadRequest(new { error = "UserId and PackageId are required" });
                }

                var subscription = await _subscriptionService.CreateSubscriptionAsync(
                    request.UserId,
                    request.PackageId,
                    request.PaymentMethodId,
                    request.StartTrial,
                    request.Currency ?? "USD");

                if (subscription == null)
                {
                    return BadRequest(new { error = "Failed to create subscription" });
                }

                var response = new SubscriptionResponse
                {
                    Id = subscription.Id,
                    UserId = subscription.UserId,
                    PackageId = subscription.PricingPackageId,
                    StripeSubscriptionId = subscription.StripeSubscriptionId,
                    Status = subscription.Status,
                    IsActive = subscription.IsActive,
                    StartDate = subscription.StartDate,
                    TrialStart = subscription.TrialStart,
                    TrialEnd = subscription.TrialEnd,
                    CurrentPeriodStart = subscription.CurrentPeriodStart,
                    CurrentPeriodEnd = subscription.CurrentPeriodEnd,
                    NextBillingDate = subscription.NextBillingDate,
                    Currency = subscription.Currency,
                    Package = subscription.Package != null ? new PackageInfo
                    {
                        Id = subscription.Package.Id,
                        Title = subscription.Package.Title,
                        Type = subscription.Package.Type,
                        Price = subscription.Package.GetPrice()
                    } : null
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription for user {UserId}", request.UserId);
                return StatusCode(500, new { error = InternalServerErrorMessage });
            }
        }

        // GET: api/Subscription/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<SubscriptionResponse>> GetUserSubscription(string userId)
        {
            try
            {
                var subscription = await _subscriptionService.GetActiveSubscriptionAsync(userId);
                if (subscription == null)
                {
                    return NotFound(new { error = "No active subscription found" });
                }

                var response = new SubscriptionResponse
                {
                    Id = subscription.Id,
                    UserId = subscription.UserId,
                    PackageId = subscription.PricingPackageId,
                    StripeSubscriptionId = subscription.StripeSubscriptionId,
                    Status = subscription.Status,
                    IsActive = subscription.IsActive,
                    StartDate = subscription.StartDate,
                    TrialStart = subscription.TrialStart,
                    TrialEnd = subscription.TrialEnd,
                    CurrentPeriodStart = subscription.CurrentPeriodStart,
                    CurrentPeriodEnd = subscription.CurrentPeriodEnd,
                    NextBillingDate = subscription.NextBillingDate,
                    Currency = subscription.Currency,
                    CancelAtPeriodEnd = subscription.CancelAtPeriodEnd,
                    Package = subscription.Package != null ? new PackageInfo
                    {
                        Id = subscription.Package.Id,
                        Title = subscription.Package.Title,
                        Type = subscription.Package.Type,
                        Price = subscription.Package.GetPrice()
                    } : null
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscription for user {UserId}", userId);
                return StatusCode(500, new { error = InternalServerErrorMessage });
            }
        }

        // POST: api/Subscription/cancel
        [HttpPost("cancel")]
        public async Task<ActionResult> CancelSubscription([FromBody] CancelSubscriptionRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var success = await _subscriptionService.CancelSubscriptionAsync(request.UserId, request.CancelImmediately);
                if (!success)
                {
                    return BadRequest(new { error = "Failed to cancel subscription" });
                }

                return Ok(new { message = "Subscription canceled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling subscription for user {UserId}", request.UserId);
                return StatusCode(500, new { error = InternalServerErrorMessage });
            }
        }

        // GET: api/Subscription/user/{userId}/details
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

        // POST: api/Subscription/reactivate
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
    public class CreateSubscriptionRequest
    {
        public string UserId { get; set; } = string.Empty;
        public int PackageId { get; set; }
        public string? PaymentMethodId { get; set; }
        public bool StartTrial { get; set; } = true;
        public string? Currency { get; set; } = "USD";
    }

    public class CancelSubscriptionRequest
    {
        public string UserId { get; set; } = string.Empty;
        public bool CancelImmediately { get; set; } = false;
    }

    public class SubscriptionActionRequest
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class SubscriptionResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int PackageId { get; set; }
        public string? StripeSubscriptionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? TrialStart { get; set; }
        public DateTime? TrialEnd { get; set; }
        public DateTime? CurrentPeriodStart { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
        public DateTime? NextBillingDate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public bool CancelAtPeriodEnd { get; set; }
        public PackageInfo? Package { get; set; }
    }

    public class PackageInfo
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }


}
