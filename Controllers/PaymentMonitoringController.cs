using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PosBackend.Models;
using PosBackend.Services;

namespace PosBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentMonitoringController : ControllerBase
    {
        private readonly PosDbContext _context;
        private readonly IPaymentMonitoringService _paymentMonitoringService;
        private readonly ILogger<PaymentMonitoringController> _logger;

        public PaymentMonitoringController(
            PosDbContext context,
            IPaymentMonitoringService paymentMonitoringService,
            ILogger<PaymentMonitoringController> logger)
        {
            _context = context;
            _paymentMonitoringService = paymentMonitoringService;
            _logger = logger;
        }

        // GET: api/PaymentMonitoring/payment-methods/{userId}
        [HttpGet("payment-methods/{userId}")]
        public async Task<ActionResult<List<PaymentMethodInfo>>> GetUserPaymentMethods(string userId)
        {
            try
            {
                var paymentMethods = await _paymentMonitoringService.GetUserPaymentMethodsAsync(userId);
                return Ok(paymentMethods);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment methods for user {UserId}", userId);
                return StatusCode(500, new { error = "Failed to retrieve payment methods" });
            }
        }

        // GET: api/PaymentMonitoring/default-payment-method/{userId}
        [HttpGet("default-payment-method/{userId}")]
        public async Task<ActionResult<PaymentMethodInfo>> GetDefaultPaymentMethod(string userId)
        {
            try
            {
                var paymentMethod = await _paymentMonitoringService.GetDefaultPaymentMethodAsync(userId);
                if (paymentMethod == null)
                {
                    return NotFound(new { error = "No default payment method found" });
                }
                return Ok(paymentMethod);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving default payment method for user {UserId}", userId);
                return StatusCode(500, new { error = "Failed to retrieve default payment method" });
            }
        }

        // POST: api/PaymentMonitoring/sync-payment-methods/{userId}
        [HttpPost("sync-payment-methods/{userId}")]
        public async Task<IActionResult> SyncPaymentMethods(string userId)
        {
            try
            {
                await _paymentMonitoringService.SyncPaymentMethodsAsync(userId);
                return Ok(new { message = "Payment methods synchronized successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing payment methods for user {UserId}", userId);
                return StatusCode(500, new { error = "Failed to sync payment methods" });
            }
        }

        // GET: api/PaymentMonitoring/notification-history/{userId}
        [HttpGet("notification-history/{userId}")]
        public async Task<ActionResult<List<PaymentNotificationHistory>>> GetNotificationHistory(
            string userId,
            [FromQuery] string? notificationType = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.PaymentNotificationHistories
                    .Where(n => n.UserId == userId);

                if (!string.IsNullOrEmpty(notificationType))
                {
                    query = query.Where(n => n.NotificationType == notificationType);
                }

                var notifications = await query
                    .OrderByDescending(n => n.SentAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification history for user {UserId}", userId);
                return StatusCode(500, new { error = "Failed to retrieve notification history" });
            }
        }

        // GET: api/PaymentMonitoring/retry-attempts/{userId}
        [HttpGet("retry-attempts/{userId}")]
        public async Task<ActionResult<List<PaymentRetryAttempt>>> GetRetryAttempts(
            string userId,
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.PaymentRetryAttempts
                    .Where(r => r.UserId == userId);

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(r => r.Status == status);
                }

                var retryAttempts = await query
                    .OrderByDescending(r => r.AttemptedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(retryAttempts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving retry attempts for user {UserId}", userId);
                return StatusCode(500, new { error = "Failed to retrieve retry attempts" });
            }
        }

        // GET: api/PaymentMonitoring/payment-health/{userId}
        [HttpGet("payment-health/{userId}")]
        public async Task<ActionResult<PaymentHealthSummary>> GetPaymentHealth(string userId)
        {
            try
            {
                var subscription = await _context.UserSubscriptions
                    .Include(s => s.Package)
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

                if (subscription == null)
                {
                    return NotFound(new { error = "No active subscription found" });
                }

                var stripeSubscription = await _context.StripeSubscriptions
                    .FirstOrDefaultAsync(s => s.StripeSubscriptionId == subscription.StripeSubscriptionId);

                var defaultPaymentMethod = await _paymentMonitoringService.GetDefaultPaymentMethodAsync(userId);

                var recentFailures = await _context.PaymentRetryAttempts
                    .Where(r => r.UserId == userId && r.AttemptedAt >= DateTime.UtcNow.AddDays(-30))
                    .CountAsync();

                var pendingRetries = await _context.PaymentRetryAttempts
                    .Where(r => r.UserId == userId && r.Status == "failed" && r.NextRetryAt > DateTime.UtcNow)
                    .CountAsync();

                var healthSummary = new PaymentHealthSummary
                {
                    UserId = userId,
                    SubscriptionStatus = subscription.Status,
                    NextBillingDate = subscription.NextBillingDate,
                    FailedPaymentAttempts = stripeSubscription?.FailedPaymentAttempts ?? 0,
                    LastFailedPaymentDate = stripeSubscription?.LastFailedPaymentDate,
                    LastFailureReason = stripeSubscription?.LastFailureReason,
                    HasDefaultPaymentMethod = defaultPaymentMethod != null,
                    PaymentMethodExpiring = defaultPaymentMethod?.IsExpiringSoon ?? false,
                    PaymentMethodExpirationDate = defaultPaymentMethod?.ExpirationDate,
                    DaysUntilExpiration = defaultPaymentMethod?.DaysUntilExpiration,
                    RecentFailuresCount = recentFailures,
                    PendingRetriesCount = pendingRetries,
                    RiskLevel = CalculateRiskLevel(stripeSubscription?.FailedPaymentAttempts ?? 0, defaultPaymentMethod)
                };

                return Ok(healthSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment health for user {UserId}", userId);
                return StatusCode(500, new { error = "Failed to retrieve payment health" });
            }
        }

        // POST: api/PaymentMonitoring/manual-retry/{retryAttemptId}
        [HttpPost("manual-retry/{retryAttemptId}")]
        public async Task<IActionResult> TriggerManualRetry(int retryAttemptId)
        {
            try
            {
                var retryAttempt = await _context.PaymentRetryAttempts
                    .FirstOrDefaultAsync(r => r.Id == retryAttemptId);

                if (retryAttempt == null)
                {
                    return NotFound(new { error = "Retry attempt not found" });
                }

                if (retryAttempt.Status != "failed")
                {
                    return BadRequest(new { error = "Can only retry failed attempts" });
                }

                // Mark as manual retry and reset next retry time
                retryAttempt.IsManualRetry = true;
                retryAttempt.NextRetryAt = DateTime.UtcNow;
                retryAttempt.LastUpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Manual retry triggered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering manual retry for attempt {RetryAttemptId}", retryAttemptId);
                return StatusCode(500, new { error = "Failed to trigger manual retry" });
            }
        }

        // POST: api/PaymentMonitoring/run-monitoring
        [HttpPost("run-monitoring")]
        [Authorize(Roles = "Admin")] // Only admins can manually trigger monitoring
        public async Task<IActionResult> RunMonitoring()
        {
            try
            {
                await Task.WhenAll(
                    _paymentMonitoringService.MonitorCardExpirationsAsync(),
                    _paymentMonitoringService.MonitorUpcomingPaymentsAsync(),
                    _paymentMonitoringService.ProcessFailedPaymentRetriesAsync()
                );

                return Ok(new { message = "Payment monitoring tasks completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running payment monitoring tasks");
                return StatusCode(500, new { error = "Failed to run monitoring tasks" });
            }
        }

        private string CalculateRiskLevel(int failedAttempts, PaymentMethodInfo? paymentMethod)
        {
            if (failedAttempts >= 3)
                return "High Risk";

            if (failedAttempts >= 1)
                return "Medium Risk";

            if (paymentMethod?.IsExpiringSoon == true && paymentMethod.DaysUntilExpiration <= 7)
                return "Medium Risk";

            if (paymentMethod?.IsExpiringSoon == true)
                return "Low Risk";

            return "Healthy";
        }
    }

    public class PaymentHealthSummary
    {
        public string UserId { get; set; } = string.Empty;
        public string SubscriptionStatus { get; set; } = string.Empty;
        public DateTime? NextBillingDate { get; set; }
        public int FailedPaymentAttempts { get; set; }
        public DateTime? LastFailedPaymentDate { get; set; }
        public string? LastFailureReason { get; set; }
        public bool HasDefaultPaymentMethod { get; set; }
        public bool PaymentMethodExpiring { get; set; }
        public DateTime? PaymentMethodExpirationDate { get; set; }
        public int? DaysUntilExpiration { get; set; }
        public int RecentFailuresCount { get; set; }
        public int PendingRetriesCount { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
    }
}
