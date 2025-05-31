using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Stripe;
using PosBackend.Models;

namespace PosBackend.Services
{
    public class SubscriptionService
    {
        private readonly PosDbContext _context;
        private readonly ILogger<SubscriptionService> _logger;
        private readonly CustomerService _stripeCustomerService;
        private readonly Stripe.SubscriptionService _stripeSubscriptionService;
        private readonly PaymentMethodService _stripePaymentMethodService;
        private readonly InvoiceService _stripeInvoiceService;

        public SubscriptionService(
            PosDbContext context,
            ILogger<SubscriptionService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;

            // Initialize Stripe services
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
            _stripeCustomerService = new CustomerService();
            _stripeSubscriptionService = new Stripe.SubscriptionService();
            _stripePaymentMethodService = new PaymentMethodService();
            _stripeInvoiceService = new InvoiceService();
        }

        public async Task<UserSubscription?> CreateSubscriptionAsync(
            string userId,
            int packageId,
            string? paymentMethodId = null,
            bool startTrial = true,
            string currency = "USD")
        {
            try
            {
                _logger.LogInformation("Creating subscription for user {UserId} with package {PackageId}", userId, packageId);

                // Get the pricing package
                var package = await _context.PricingPackages.FindAsync(packageId);
                if (package == null)
                {
                    _logger.LogError("Package {PackageId} not found", packageId);
                    return null;
                }

                // Check if user already has an active subscription
                var existingSubscription = await GetActiveSubscriptionAsync(userId);
                if (existingSubscription != null)
                {
                    _logger.LogWarning("User {UserId} already has an active subscription", userId);
                    // Optionally handle subscription upgrade/downgrade here
                    return existingSubscription;
                }

                // Create or get Stripe customer
                var stripeCustomer = await GetOrCreateStripeCustomerAsync(userId);
                if (stripeCustomer == null)
                {
                    _logger.LogError("Failed to create Stripe customer for user {UserId}", userId);
                    return null;
                }

                // Get the appropriate Stripe price ID
                var stripePriceId = GetStripePriceId(package, currency);
                if (string.IsNullOrEmpty(stripePriceId))
                {
                    _logger.LogError("No Stripe price ID found for package {PackageId} and currency {Currency}", packageId, currency);
                    return null;
                }

                // Create Stripe subscription
                var subscriptionCreateOptions = new SubscriptionCreateOptions
                {
                    Customer = stripeCustomer.Id,
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions
                        {
                            Price = stripePriceId,
                        }
                    },
                    PaymentBehavior = "default_incomplete",
                    PaymentSettings = new SubscriptionPaymentSettingsOptions
                    {
                        SaveDefaultPaymentMethod = "on_subscription"
                    },
                    Expand = new List<string> { "latest_invoice.payment_intent" },
                    Metadata = new Dictionary<string, string>
                    {
                        { "user_id", userId },
                        { "package_id", packageId.ToString() },
                        { "package_type", package.Type }
                    }
                };

                // Add trial period if applicable
                if (startTrial && package.TestPeriodDays > 0)
                {
                    subscriptionCreateOptions.TrialPeriodDays = package.TestPeriodDays;
                }

                // Add payment method if provided
                if (!string.IsNullOrEmpty(paymentMethodId))
                {
                    subscriptionCreateOptions.DefaultPaymentMethod = paymentMethodId;
                }

                var stripeSubscription = await _stripeSubscriptionService.CreateAsync(subscriptionCreateOptions);

                // Create local subscription record
                var userSubscription = new UserSubscription
                {
                    UserId = userId,
                    PricingPackageId = packageId,
                    StripeSubscriptionId = stripeSubscription.Id,
                    StripeCustomerId = stripeCustomer.Id,
                    StripePriceId = stripePriceId,
                    Status = stripeSubscription.Status,
                    StartDate = DateTime.UtcNow,
                    IsActive = stripeSubscription.Status == "active" || stripeSubscription.Status == "trialing",
                    Currency = currency.ToUpper(),
                    TrialStart = stripeSubscription.TrialStart,
                    TrialEnd = stripeSubscription.TrialEnd,
                    CurrentPeriodStart = GetCurrentPeriodStart(stripeSubscription),
                    CurrentPeriodEnd = GetCurrentPeriodEnd(stripeSubscription),
                    NextBillingDate = GetNextBillingDate(stripeSubscription),
                    CancelAtPeriodEnd = stripeSubscription.CancelAtPeriodEnd,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                };

                _context.UserSubscriptions.Add(userSubscription);
                await _context.SaveChangesAsync();

                // Create Stripe subscription record
                var stripeSubscriptionRecord = new StripeSubscription
                {
                    StripeSubscriptionId = stripeSubscription.Id,
                    StripeCustomerId = stripeCustomer.Id,
                    UserId = userId,
                    UserSubscriptionId = userSubscription.Id,
                    StripePriceId = stripePriceId,
                    StripeProductId = package.StripeProductId ?? "",
                    Status = stripeSubscription.Status,
                    Amount = package.Price,
                    Currency = currency.ToUpper(),
                    BillingInterval = package.BillingInterval,
                    BillingIntervalCount = package.BillingIntervalCount,
                    TrialStart = stripeSubscription.TrialStart,
                    TrialEnd = stripeSubscription.TrialEnd,
                    CurrentPeriodStart = GetCurrentPeriodStart(stripeSubscription),
                    CurrentPeriodEnd = GetCurrentPeriodEnd(stripeSubscription),
                    CancelAtPeriodEnd = stripeSubscription.CancelAtPeriodEnd,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow,
                    LastSyncedAt = DateTime.UtcNow
                };

                _context.StripeSubscriptions.Add(stripeSubscriptionRecord);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created subscription {SubscriptionId} for user {UserId}", stripeSubscription.Id, userId);

                return userSubscription;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription for user {UserId} with package {PackageId}", userId, packageId);
                return null;
            }
        }

        public async Task<UserSubscription?> GetActiveSubscriptionAsync(string userId)
        {
            return await _context.UserSubscriptions
                .Include(s => s.Package)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);
        }

        public async Task<bool> CancelSubscriptionAsync(string userId, bool cancelImmediately = false)
        {
            try
            {
                var subscription = await GetActiveSubscriptionAsync(userId);
                if (subscription == null || string.IsNullOrEmpty(subscription.StripeSubscriptionId))
                {
                    _logger.LogWarning("No active subscription found for user {UserId}", userId);
                    return false;
                }

                var cancelOptions = new SubscriptionCancelOptions();
                if (!cancelImmediately)
                {
                    // Cancel at period end
                    var updateOptions = new SubscriptionUpdateOptions
                    {
                        CancelAtPeriodEnd = true
                    };
                    await _stripeSubscriptionService.UpdateAsync(subscription.StripeSubscriptionId, updateOptions);

                    subscription.CancelAtPeriodEnd = true;
                }
                else
                {
                    // Cancel immediately
                    await _stripeSubscriptionService.CancelAsync(subscription.StripeSubscriptionId, cancelOptions);

                    subscription.IsActive = false;
                    subscription.Status = "canceled";
                    subscription.CanceledAt = DateTime.UtcNow;
                    subscription.EndDate = DateTime.UtcNow;
                }

                subscription.LastUpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully canceled subscription for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling subscription for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ReactivateSubscriptionAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Reactivating subscription for user {UserId}", userId);

                var subscription = await GetActiveSubscriptionAsync(userId);
                if (subscription == null)
                {
                    _logger.LogWarning("No subscription found for user {UserId}", userId);
                    return false;
                }

                if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
                {
                    await _stripeSubscriptionService.UpdateAsync(subscription.StripeSubscriptionId, new SubscriptionUpdateOptions
                    {
                        CancelAtPeriodEnd = false
                    });
                }

                subscription.CancelAtPeriodEnd = false;
                subscription.CanceledAt = null;
                subscription.LastUpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Subscription reactivated successfully for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating subscription for user {UserId}", userId);
                return false;
            }
        }

        public async Task<UserSubscription?> ChangeSubscriptionPlanAsync(string userId, int newPackageId, bool prorated = true)
        {
            try
            {
                _logger.LogInformation("Changing subscription plan for user {UserId} to package {PackageId}", userId, newPackageId);

                var currentSubscription = await GetActiveSubscriptionAsync(userId);
                if (currentSubscription == null)
                {
                    _logger.LogWarning("No active subscription found for user {UserId}", userId);
                    return null;
                }

                var newPackage = await _context.PricingPackages.FindAsync(newPackageId);
                if (newPackage == null)
                {
                    _logger.LogError("Package {PackageId} not found", newPackageId);
                    return null;
                }

                var stripePriceId = GetStripePriceId(newPackage, currentSubscription.Currency);
                if (string.IsNullOrEmpty(stripePriceId))
                {
                    _logger.LogError("No Stripe price ID found for package {PackageId} in currency {Currency}", newPackageId, currentSubscription.Currency);
                    return null;
                }

                // Update Stripe subscription
                if (!string.IsNullOrEmpty(currentSubscription.StripeSubscriptionId))
                {
                    var stripeSubscription = await _stripeSubscriptionService.UpdateAsync(currentSubscription.StripeSubscriptionId, new SubscriptionUpdateOptions
                    {
                        Items = new List<SubscriptionItemOptions>
                        {
                            new SubscriptionItemOptions
                            {
                                Id = (await _stripeSubscriptionService.GetAsync(currentSubscription.StripeSubscriptionId)).Items.Data[0].Id,
                                Price = stripePriceId
                            }
                        },
                        ProrationBehavior = prorated ? "create_prorations" : "none"
                    });

                    // Update local subscription record
                    currentSubscription.PricingPackageId = newPackageId;
                    currentSubscription.StripePriceId = stripePriceId;
                    currentSubscription.Status = stripeSubscription.Status;
                    currentSubscription.CurrentPeriodStart = GetCurrentPeriodStart(stripeSubscription);
                    currentSubscription.CurrentPeriodEnd = GetCurrentPeriodEnd(stripeSubscription);
                    currentSubscription.NextBillingDate = GetNextBillingDate(stripeSubscription);
                    currentSubscription.LastUpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Subscription plan changed successfully for user {UserId}", userId);
                    return currentSubscription;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing subscription plan for user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> UpdatePaymentMethodAsync(string userId, string paymentMethodId)
        {
            try
            {
                _logger.LogInformation("Updating payment method for user {UserId}", userId);

                var subscription = await GetActiveSubscriptionAsync(userId);
                if (subscription == null)
                {
                    _logger.LogWarning("No active subscription found for user {UserId}", userId);
                    return false;
                }

                if (!string.IsNullOrEmpty(subscription.StripeCustomerId))
                {
                    // Attach payment method to customer
                    await _stripePaymentMethodService.AttachAsync(paymentMethodId, new PaymentMethodAttachOptions
                    {
                        Customer = subscription.StripeCustomerId
                    });

                    // Set as default payment method
                    await _stripeCustomerService.UpdateAsync(subscription.StripeCustomerId, new CustomerUpdateOptions
                    {
                        InvoiceSettings = new CustomerInvoiceSettingsOptions
                        {
                            DefaultPaymentMethod = paymentMethodId
                        }
                    });

                    // Update subscription's default payment method
                    if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
                    {
                        await _stripeSubscriptionService.UpdateAsync(subscription.StripeSubscriptionId, new SubscriptionUpdateOptions
                        {
                            DefaultPaymentMethod = paymentMethodId
                        });
                    }

                    _logger.LogInformation("Payment method updated successfully for user {UserId}", userId);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment method for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> PauseSubscriptionAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Pausing subscription for user {UserId}", userId);

                var subscription = await GetActiveSubscriptionAsync(userId);
                if (subscription == null)
                {
                    _logger.LogWarning("No active subscription found for user {UserId}", userId);
                    return false;
                }

                if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
                {
                    await _stripeSubscriptionService.UpdateAsync(subscription.StripeSubscriptionId, new SubscriptionUpdateOptions
                    {
                        PauseCollection = new SubscriptionPauseCollectionOptions
                        {
                            Behavior = "void"
                        }
                    });
                }

                subscription.Status = "paused";
                subscription.LastUpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Subscription paused successfully for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing subscription for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ResumeSubscriptionAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Resuming subscription for user {UserId}", userId);

                var subscription = await GetActiveSubscriptionAsync(userId);
                if (subscription == null)
                {
                    _logger.LogWarning("No subscription found for user {UserId}", userId);
                    return false;
                }

                if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
                {
                    await _stripeSubscriptionService.UpdateAsync(subscription.StripeSubscriptionId, new SubscriptionUpdateOptions
                    {
                        PauseCollection = null
                    });
                }

                subscription.Status = "active";
                subscription.LastUpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Subscription resumed successfully for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming subscription for user {UserId}", userId);
                return false;
            }
        }

        public async Task<List<object>> GetBillingHistoryAsync(string userId, int limit = 10)
        {
            try
            {
                _logger.LogInformation("Getting billing history for user {UserId}", userId);

                var subscription = await GetActiveSubscriptionAsync(userId);
                if (subscription == null || string.IsNullOrEmpty(subscription.StripeCustomerId))
                {
                    _logger.LogWarning("No subscription or customer found for user {UserId}", userId);
                    return new List<object>();
                }

                var invoices = await _stripeInvoiceService.ListAsync(new InvoiceListOptions
                {
                    Customer = subscription.StripeCustomerId,
                    Limit = limit,
                    Status = "paid"
                });

                var billingHistory = invoices.Data.Select(invoice => new
                {
                    Id = invoice.Id,
                    Amount = invoice.AmountPaid / 100.0m, // Convert from cents
                    Currency = invoice.Currency.ToUpper(),
                    Date = invoice.Created,
                    Status = invoice.Status,
                    InvoiceUrl = invoice.HostedInvoiceUrl,
                    InvoicePdf = invoice.InvoicePdf,
                    Description = invoice.Lines?.Data?.FirstOrDefault()?.Description ?? "Subscription"
                }).Cast<object>().ToList();

                return billingHistory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting billing history for user {UserId}", userId);
                return new List<object>();
            }
        }

        public async Task<object?> GetSubscriptionDetailsAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Getting subscription details for user {UserId}", userId);

                var subscription = await _context.UserSubscriptions
                    .Include(s => s.Package)
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

                if (subscription == null)
                {
                    return null;
                }

                // Get payment method info from Stripe if available
                object? paymentMethod = null;
                if (!string.IsNullOrEmpty(subscription.StripeCustomerId))
                {
                    try
                    {
                        var customer = await _stripeCustomerService.GetAsync(subscription.StripeCustomerId, new CustomerGetOptions
                        {
                            Expand = new List<string> { "invoice_settings.default_payment_method" }
                        });

                        var defaultPaymentMethod = customer.InvoiceSettings?.DefaultPaymentMethod;
                        if (defaultPaymentMethod != null)
                        {
                            paymentMethod = new
                            {
                                Type = defaultPaymentMethod.Type,
                                Card = defaultPaymentMethod.Card != null ? new
                                {
                                    Brand = defaultPaymentMethod.Card.Brand,
                                    Last4 = defaultPaymentMethod.Card.Last4,
                                    ExpMonth = defaultPaymentMethod.Card.ExpMonth,
                                    ExpYear = defaultPaymentMethod.Card.ExpYear
                                } : null
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not retrieve payment method for user {UserId}", userId);
                    }
                }

                return new
                {
                    Id = subscription.Id,
                    UserId = subscription.UserId,
                    Package = new
                    {
                        Id = subscription.Package?.Id,
                        Title = subscription.Package?.Title,
                        Type = subscription.Package?.Type,
                        Price = subscription.Package?.Price
                    },
                    Status = subscription.Status,
                    IsActive = subscription.IsActive,
                    StartDate = subscription.StartDate,
                    EndDate = subscription.EndDate,
                    TrialStart = subscription.TrialStart,
                    TrialEnd = subscription.TrialEnd,
                    CurrentPeriodStart = subscription.CurrentPeriodStart,
                    CurrentPeriodEnd = subscription.CurrentPeriodEnd,
                    NextBillingDate = subscription.NextBillingDate,
                    CancelAtPeriodEnd = subscription.CancelAtPeriodEnd,
                    CanceledAt = subscription.CanceledAt,
                    Currency = subscription.Currency,
                    PaymentMethod = paymentMethod,
                    EnabledFeatures = subscription.EnabledFeatures
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription details for user {UserId}", userId);
                return null;
            }
        }

        private async Task<Stripe.Customer?> GetOrCreateStripeCustomerAsync(string userId)
        {
            try
            {
                // Check if customer already exists in our database
                var existingSubscription = await _context.UserSubscriptions
                    .FirstOrDefaultAsync(s => s.UserId == userId && !string.IsNullOrEmpty(s.StripeCustomerId));

                if (existingSubscription != null && !string.IsNullOrEmpty(existingSubscription.StripeCustomerId))
                {
                    try
                    {
                        return await _stripeCustomerService.GetAsync(existingSubscription.StripeCustomerId);
                    }
                    catch (StripeException)
                    {
                        // Customer doesn't exist in Stripe, create a new one
                    }
                }

                // Create new Stripe customer
                var customerOptions = new CustomerCreateOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { "user_id", userId }
                    }
                };

                return await _stripeCustomerService.CreateAsync(customerOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Stripe customer for user {UserId}", userId);
                return null;
            }
        }

        private string? GetStripePriceId(PricingPackage package, string currency)
        {
            if (currency.ToUpper() == "USD" && !string.IsNullOrEmpty(package.StripePriceId))
            {
                return package.StripePriceId;
            }

            // Try to get price ID for specific currency
            try
            {
                var multiCurrencyPrices = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(
                    package.StripeMultiCurrencyPriceIds ?? "{}");

                if (multiCurrencyPrices?.TryGetValue(currency.ToUpper(), out var priceId) == true)
                {
                    return priceId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing multi-currency price IDs for package {PackageId}", package.Id);
            }

            // Fallback to USD price
            return package.StripePriceId;
        }

        /// <summary>
        /// Extracts current period start from Stripe subscription.
        /// Uses BillingCycleAnchor which represents the current period start.
        /// </summary>
        private static DateTime? GetCurrentPeriodStart(Stripe.Subscription subscription)
        {
            // Use billing cycle anchor which represents the current period start
            return subscription.BillingCycleAnchor;
        }

        /// <summary>
        /// Extracts current period end from Stripe subscription.
        /// Calculates based on billing cycle anchor and interval.
        /// </summary>
        private static DateTime? GetCurrentPeriodEnd(Stripe.Subscription subscription)
        {
            if (!(subscription.Items?.Data?.Any() ?? false))
                return null;

            var firstItem = subscription.Items.Data.First();
            var price = firstItem.Price;

            if (price?.Recurring == null)
                return null;

            var startDate = subscription.BillingCycleAnchor;
            var interval = price.Recurring.Interval;
            var intervalCount = (int)(price.Recurring.IntervalCount > 0 ? price.Recurring.IntervalCount : 1);

            return interval switch
            {
                "day" => startDate.AddDays(intervalCount),
                "week" => startDate.AddDays(intervalCount * 7),
                "month" => startDate.AddMonths(intervalCount),
                "year" => startDate.AddYears(intervalCount),
                _ => (DateTime?)null
            };
        }

        /// <summary>
        /// Calculates the next billing date based on current period end.
        /// </summary>
        private static DateTime? GetNextBillingDate(Stripe.Subscription subscription)
        {
            // For active subscriptions, next billing date is the current period end
            if (subscription.Status == "active" || subscription.Status == "trialing")
            {
                return GetCurrentPeriodEnd(subscription);
            }

            return null;
        }
    }
}
