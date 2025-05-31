using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using PosBackend.Models;

namespace PosBackend.Services
{
    public interface IPaymentMonitoringService
    {
        Task MonitorCardExpirationsAsync();
        Task MonitorUpcomingPaymentsAsync();
        Task ProcessFailedPaymentRetriesAsync();
        Task SyncPaymentMethodsAsync(string userId);
        Task<PaymentMethodInfo?> GetDefaultPaymentMethodAsync(string userId);
        Task<List<PaymentMethodInfo>> GetUserPaymentMethodsAsync(string userId);
        Task UpdatePaymentMethodInfoAsync(string stripePaymentMethodId);
    }

    public class PaymentMonitoringService : IPaymentMonitoringService
    {
        private readonly PosDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<PaymentMonitoringService> _logger;
        private readonly IConfiguration _configuration;
        private readonly CustomerService _stripeCustomerService;
        private readonly PaymentMethodService _stripePaymentMethodService;
        private readonly PaymentMonitoringConfiguration _config;

        public PaymentMonitoringService(
            PosDbContext context,
            IEmailService emailService,
            ILogger<PaymentMonitoringService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;

            // Initialize Stripe services
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
            _stripeCustomerService = new CustomerService();
            _stripePaymentMethodService = new PaymentMethodService();

            _config = new PaymentMonitoringConfiguration();
            _configuration.GetSection("PaymentMonitoring").Bind(_config);
        }

        public async Task MonitorCardExpirationsAsync()
        {
            try
            {
                _logger.LogInformation("Starting card expiration monitoring");

                var today = DateTime.UtcNow.Date;
                var paymentMethods = await _context.PaymentMethodInfos
                    .Where(pm => pm.IsActive && pm.ExpirationDate.HasValue)
                    .ToListAsync();

                foreach (var paymentMethod in paymentMethods)
                {
                    var daysUntilExpiration = paymentMethod.DaysUntilExpiration;

                    // Check for 30-day warning
                    if (daysUntilExpiration <= 30 && daysUntilExpiration > 7 && !paymentMethod.ExpirationWarning30DaysSent)
                    {
                        await SendCardExpirationWarning(paymentMethod, 30);
                        paymentMethod.ExpirationWarning30DaysSent = true;
                    }
                    // Check for 7-day warning
                    else if (daysUntilExpiration <= 7 && daysUntilExpiration > 1 && !paymentMethod.ExpirationWarning7DaysSent)
                    {
                        await SendCardExpirationWarning(paymentMethod, 7);
                        paymentMethod.ExpirationWarning7DaysSent = true;
                    }
                    // Check for 1-day warning
                    else if (daysUntilExpiration <= 1 && !paymentMethod.ExpirationWarning1DaySent)
                    {
                        await SendCardExpirationWarning(paymentMethod, 1);
                        paymentMethod.ExpirationWarning1DaySent = true;
                    }

                    paymentMethod.LastUpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Card expiration monitoring completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during card expiration monitoring");
            }
        }

        public async Task MonitorUpcomingPaymentsAsync()
        {
            try
            {
                _logger.LogInformation("Starting upcoming payment monitoring");

                var today = DateTime.UtcNow.Date;
                var upcomingSubscriptions = await _context.UserSubscriptions
                    .Where(s => s.IsActive && s.NextBillingDate.HasValue)
                    .Include(s => s.Package)
                    .ToListAsync();

                foreach (var subscription in upcomingSubscriptions)
                {
                    if (!subscription.NextBillingDate.HasValue) continue;

                    var daysUntilBilling = (int)(subscription.NextBillingDate.Value.Date - today).TotalDays;

                    if (_config.UpcomingPaymentReminderDays.Contains(daysUntilBilling))
                    {
                        // Check if we already sent a notification for this billing date and reminder type
                        var existingNotification = await _context.PaymentNotificationHistories
                            .AnyAsync(n => n.UserId == subscription.UserId &&
                                         n.NotificationType == "UpcomingPayment" &&
                                         n.SentAt.Date == today &&
                                         n.ContextData!.Contains(subscription.NextBillingDate.Value.ToString("yyyy-MM-dd")));

                        if (!existingNotification)
                        {
                            await SendUpcomingPaymentReminder(subscription, daysUntilBilling);
                        }
                    }
                }

                _logger.LogInformation("Upcoming payment monitoring completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during upcoming payment monitoring");
            }
        }

        public async Task ProcessFailedPaymentRetriesAsync()
        {
            try
            {
                _logger.LogInformation("Starting failed payment retry processing");

                var retryAttempts = await _context.PaymentRetryAttempts
                    .Where(r => r.Status == "failed" && r.IsRetryDue && !r.IsMaxRetriesReached)
                    .ToListAsync();

                foreach (var retryAttempt in retryAttempts)
                {
                    await ProcessRetryAttempt(retryAttempt);
                }

                _logger.LogInformation("Failed payment retry processing completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during failed payment retry processing");
            }
        }

        public async Task SyncPaymentMethodsAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Syncing payment methods for user {UserId}", userId);

                // Get user's Stripe customer ID
                var subscription = await _context.UserSubscriptions
                    .FirstOrDefaultAsync(s => s.UserId == userId && !string.IsNullOrEmpty(s.StripeCustomerId));

                if (subscription?.StripeCustomerId == null)
                {
                    _logger.LogWarning("No Stripe customer found for user {UserId}", userId);
                    return;
                }

                // Get payment methods from Stripe
                var paymentMethods = await _stripePaymentMethodService.ListAsync(new PaymentMethodListOptions
                {
                    Customer = subscription.StripeCustomerId,
                    Type = "card"
                });

                // Update or create payment method records
                foreach (var stripePaymentMethod in paymentMethods)
                {
                    await UpdateOrCreatePaymentMethodInfo(userId, subscription.StripeCustomerId, stripePaymentMethod);
                }

                // Mark inactive payment methods that are no longer in Stripe
                var stripePaymentMethodIds = paymentMethods.Select(pm => pm.Id).ToList();
                var localPaymentMethods = await _context.PaymentMethodInfos
                    .Where(pm => pm.UserId == userId && !stripePaymentMethodIds.Contains(pm.StripePaymentMethodId))
                    .ToListAsync();

                foreach (var localPaymentMethod in localPaymentMethods)
                {
                    localPaymentMethod.IsActive = false;
                    localPaymentMethod.LastUpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Payment methods synced for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing payment methods for user {UserId}", userId);
            }
        }

        public async Task<PaymentMethodInfo?> GetDefaultPaymentMethodAsync(string userId)
        {
            return await _context.PaymentMethodInfos
                .FirstOrDefaultAsync(pm => pm.UserId == userId && pm.IsDefault && pm.IsActive);
        }

        public async Task<List<PaymentMethodInfo>> GetUserPaymentMethodsAsync(string userId)
        {
            return await _context.PaymentMethodInfos
                .Where(pm => pm.UserId == userId && pm.IsActive)
                .OrderByDescending(pm => pm.IsDefault)
                .ThenByDescending(pm => pm.LastUsedAt)
                .ToListAsync();
        }

        public async Task UpdatePaymentMethodInfoAsync(string stripePaymentMethodId)
        {
            try
            {
                var paymentMethod = await _stripePaymentMethodService.GetAsync(stripePaymentMethodId);
                var localPaymentMethod = await _context.PaymentMethodInfos
                    .FirstOrDefaultAsync(pm => pm.StripePaymentMethodId == stripePaymentMethodId);

                if (localPaymentMethod != null && paymentMethod.Card != null)
                {
                    UpdatePaymentMethodFromStripe(localPaymentMethod, paymentMethod);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment method info for {PaymentMethodId}", stripePaymentMethodId);
            }
        }

        private async Task SendCardExpirationWarning(PaymentMethodInfo paymentMethod, int daysUntilExpiration)
        {
            try
            {
                // Get user email (you might need to implement this based on your user system)
                var userEmail = await GetUserEmailAsync(paymentMethod.UserId);
                var userName = await GetUserNameAsync(paymentMethod.UserId);

                if (!string.IsNullOrEmpty(userEmail) && paymentMethod.ExpirationDate.HasValue)
                {
                    var success = await _emailService.SendCardExpirationWarningAsync(
                        userEmail, userName, paymentMethod.ExpirationDate.Value, daysUntilExpiration);

                    // Log notification
                    await LogNotification(paymentMethod.UserId, "CardExpiration",
                        $"Card expiring in {daysUntilExpiration} days", userEmail, success);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending card expiration warning for payment method {PaymentMethodId}",
                    paymentMethod.StripePaymentMethodId);
            }
        }

        private async Task SendUpcomingPaymentReminder(UserSubscription subscription, int daysUntilBilling)
        {
            try
            {
                var userEmail = await GetUserEmailAsync(subscription.UserId);
                var userName = await GetUserNameAsync(subscription.UserId);

                if (!string.IsNullOrEmpty(userEmail) && subscription.NextBillingDate.HasValue && subscription.Package != null)
                {
                    var success = await _emailService.SendUpcomingPaymentReminderAsync(
                        userEmail, userName, subscription.Package.Price, subscription.NextBillingDate.Value, daysUntilBilling);

                    // Log notification
                    await LogNotification(subscription.UserId, "UpcomingPayment",
                        $"Payment reminder - {daysUntilBilling} days", userEmail, success,
                        contextData: $"{{\"billingDate\":\"{subscription.NextBillingDate.Value:yyyy-MM-dd}\",\"amount\":{subscription.Package.Price}}}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending upcoming payment reminder for subscription {SubscriptionId}",
                    subscription.StripeSubscriptionId);
            }
        }

        private async Task ProcessRetryAttempt(PaymentRetryAttempt retryAttempt)
        {
            try
            {
                // This would integrate with Stripe to actually retry the payment
                // For now, we'll just log and update the retry attempt
                _logger.LogInformation("Processing retry attempt {AttemptId} for subscription {SubscriptionId}",
                    retryAttempt.Id, retryAttempt.StripeSubscriptionId);

                // Update next retry time
                retryAttempt.AttemptNumber++;
                retryAttempt.CalculateNextRetryTime(_config.FailedPaymentRetryIntervalHours);
                retryAttempt.LastUpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing retry attempt {AttemptId}", retryAttempt.Id);
            }
        }

        private async Task UpdateOrCreatePaymentMethodInfo(string userId, string stripeCustomerId, PaymentMethod stripePaymentMethod)
        {
            var existingPaymentMethod = await _context.PaymentMethodInfos
                .FirstOrDefaultAsync(pm => pm.StripePaymentMethodId == stripePaymentMethod.Id);

            if (existingPaymentMethod != null)
            {
                UpdatePaymentMethodFromStripe(existingPaymentMethod, stripePaymentMethod);
            }
            else
            {
                var newPaymentMethod = CreatePaymentMethodFromStripe(userId, stripeCustomerId, stripePaymentMethod);
                _context.PaymentMethodInfos.Add(newPaymentMethod);
            }
        }

        private void UpdatePaymentMethodFromStripe(PaymentMethodInfo localPaymentMethod, PaymentMethod stripePaymentMethod)
        {
            if (stripePaymentMethod.Card != null)
            {
                localPaymentMethod.CardBrand = stripePaymentMethod.Card.Brand;
                localPaymentMethod.CardLast4 = stripePaymentMethod.Card.Last4;
                localPaymentMethod.CardExpMonth = (int)stripePaymentMethod.Card.ExpMonth;
                localPaymentMethod.CardExpYear = (int)stripePaymentMethod.Card.ExpYear;
                localPaymentMethod.CardCountry = stripePaymentMethod.Card.Country;
                localPaymentMethod.CardFunding = stripePaymentMethod.Card.Funding;
                localPaymentMethod.UpdateExpirationDate();
            }

            localPaymentMethod.LastUpdatedAt = DateTime.UtcNow;
        }

        private PaymentMethodInfo CreatePaymentMethodFromStripe(string userId, string stripeCustomerId, PaymentMethod stripePaymentMethod)
        {
            var paymentMethodInfo = new PaymentMethodInfo
            {
                UserId = userId,
                StripeCustomerId = stripeCustomerId,
                StripePaymentMethodId = stripePaymentMethod.Id,
                Type = stripePaymentMethod.Type,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            if (stripePaymentMethod.Card != null)
            {
                paymentMethodInfo.CardBrand = stripePaymentMethod.Card.Brand;
                paymentMethodInfo.CardLast4 = stripePaymentMethod.Card.Last4;
                paymentMethodInfo.CardExpMonth = (int)stripePaymentMethod.Card.ExpMonth;
                paymentMethodInfo.CardExpYear = (int)stripePaymentMethod.Card.ExpYear;
                paymentMethodInfo.CardCountry = stripePaymentMethod.Card.Country;
                paymentMethodInfo.CardFunding = stripePaymentMethod.Card.Funding;
                paymentMethodInfo.UpdateExpirationDate();
            }

            return paymentMethodInfo;
        }

        private async Task LogNotification(string userId, string notificationType, string message, string recipient,
            bool success, string? contextData = null)
        {
            var notification = new PaymentNotificationHistory
            {
                UserId = userId,
                NotificationType = notificationType,
                Subject = $"Payment Notification - {notificationType}",
                Message = message,
                DeliveryMethod = "Email",
                Recipient = recipient,
                Status = success ? "Sent" : "Failed",
                SentAt = DateTime.UtcNow,
                ContextData = contextData
            };

            _context.PaymentNotificationHistories.Add(notification);
            await _context.SaveChangesAsync();
        }

        private Task<string> GetUserEmailAsync(string userId)
        {
            // This should be implemented based on your user management system
            // For now, returning a placeholder
            return Task.FromResult("user@example.com");
        }

        private Task<string> GetUserNameAsync(string userId)
        {
            // This should be implemented based on your user management system
            // For now, returning a placeholder
            return Task.FromResult("User");
        }
    }

    public class PaymentMonitoringConfiguration
    {
        public int[] CardExpirationWarningDays { get; set; } = { 30, 7, 1 };
        public int[] UpcomingPaymentReminderDays { get; set; } = { 7, 1 };
        public int[] FailedPaymentRetryIntervalHours { get; set; } = { 1, 6, 24, 72 };
        public int MaxRetryAttempts { get; set; } = 4;
        public bool EnableProactiveMonitoring { get; set; } = true;
        public int MonitoringIntervalMinutes { get; set; } = 60;
    }
}
