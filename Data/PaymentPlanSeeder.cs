using Microsoft.EntityFrameworkCore;
using PosBackend.Models;
using POS.Models;

namespace PosBackend.Data
{
    public static class PaymentPlanSeeder
    {
        public static async Task SeedPaymentPlansAsync(PosDbContext context)
        {
            // Check if payment plans already exist
            if (await context.PaymentPlans.AnyAsync())
            {
                Console.WriteLine("Payment plans already exist. Skipping seeding.");
                return;
            }

            Console.WriteLine("Seeding payment plans...");

            var paymentPlans = new List<PaymentPlan>
            {
                // USD Payment Plans
                new PaymentPlan
                {
                    Name = "Monthly",
                    Period = "1 month",
                    DiscountPercentage = 0.0m,
                    DiscountLabel = null,
                    Description = "Pay monthly with full flexibility",
                    IsPopular = false,
                    IsDefault = true,
                    ValidFrom = null,
                    ValidTo = null,
                    ApplicableRegions = "*",
                    ApplicableUserTypes = "*",
                    Currency = "USD",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new PaymentPlan
                {
                    Name = "Quarterly",
                    Period = "3 months",
                    DiscountPercentage = 0.10m,
                    DiscountLabel = "10% OFF",
                    Description = "Save 10% with quarterly billing",
                    IsPopular = false,
                    IsDefault = false,
                    ValidFrom = null,
                    ValidTo = null,
                    ApplicableRegions = "*",
                    ApplicableUserTypes = "*",
                    Currency = "USD",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new PaymentPlan
                {
                    Name = "Semi-Annual",
                    Period = "6 months",
                    DiscountPercentage = 0.15m,
                    DiscountLabel = "15% OFF",
                    Description = "Save 15% with semi-annual billing",
                    IsPopular = true,
                    IsDefault = false,
                    ValidFrom = null,
                    ValidTo = null,
                    ApplicableRegions = "*",
                    ApplicableUserTypes = "*",
                    Currency = "USD",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new PaymentPlan
                {
                    Name = "Annual",
                    Period = "12 months",
                    DiscountPercentage = 0.20m,
                    DiscountLabel = "20% OFF",
                    Description = "Maximum savings with annual billing",
                    IsPopular = false,
                    IsDefault = false,
                    ValidFrom = null,
                    ValidTo = null,
                    ApplicableRegions = "*",
                    ApplicableUserTypes = "*",
                    Currency = "USD",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                // EUR Payment Plans
                new PaymentPlan
                {
                    Name = "Monthly",
                    Period = "1 month",
                    DiscountPercentage = 0.0m,
                    DiscountLabel = null,
                    Description = "Pay monthly with full flexibility",
                    IsPopular = false,
                    IsDefault = true,
                    ValidFrom = null,
                    ValidTo = null,
                    ApplicableRegions = "*",
                    ApplicableUserTypes = "*",
                    Currency = "EUR",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new PaymentPlan
                {
                    Name = "Quarterly",
                    Period = "3 months",
                    DiscountPercentage = 0.10m,
                    DiscountLabel = "10% OFF",
                    Description = "Save 10% with quarterly billing",
                    IsPopular = false,
                    IsDefault = false,
                    ValidFrom = null,
                    ValidTo = null,
                    ApplicableRegions = "*",
                    ApplicableUserTypes = "*",
                    Currency = "EUR",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new PaymentPlan
                {
                    Name = "Semi-Annual",
                    Period = "6 months",
                    DiscountPercentage = 0.15m,
                    DiscountLabel = "15% OFF",
                    Description = "Save 15% with semi-annual billing",
                    IsPopular = true,
                    IsDefault = false,
                    ValidFrom = null,
                    ValidTo = null,
                    ApplicableRegions = "*",
                    ApplicableUserTypes = "*",
                    Currency = "EUR",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new PaymentPlan
                {
                    Name = "Annual",
                    Period = "12 months",
                    DiscountPercentage = 0.20m,
                    DiscountLabel = "20% OFF",
                    Description = "Maximum savings with annual billing",
                    IsPopular = false,
                    IsDefault = false,
                    ValidFrom = null,
                    ValidTo = null,
                    ApplicableRegions = "*",
                    ApplicableUserTypes = "*",
                    Currency = "EUR",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                // ZAR Payment Plans
                new PaymentPlan
                {
                    Name = "Monthly",
                    Period = "1 month",
                    DiscountPercentage = 0.0m,
                    DiscountLabel = null,
                    Description = "Pay monthly with full flexibility",
                    IsPopular = false,
                    IsDefault = true,
                    ValidFrom = null,
                    ValidTo = null,
                    ApplicableRegions = "*",
                    ApplicableUserTypes = "*",
                    Currency = "ZAR",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new PaymentPlan
                {
                    Name = "Quarterly",
                    Period = "3 months",
                    DiscountPercentage = 0.10m,
                    DiscountLabel = "10% OFF",
                    Description = "Save 10% with quarterly billing",
                    IsPopular = false,
                    IsDefault = false,
                    ValidFrom = null,
                    ValidTo = null,
                    ApplicableRegions = "*",
                    ApplicableUserTypes = "*",
                    Currency = "ZAR",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new PaymentPlan
                {
                    Name = "Semi-Annual",
                    Period = "6 months",
                    DiscountPercentage = 0.15m,
                    DiscountLabel = "15% OFF",
                    Description = "Save 15% with semi-annual billing",
                    IsPopular = true,
                    IsDefault = false,
                    ValidFrom = null,
                    ValidTo = null,
                    ApplicableRegions = "*",
                    ApplicableUserTypes = "*",
                    Currency = "ZAR",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new PaymentPlan
                {
                    Name = "Annual",
                    Period = "12 months",
                    DiscountPercentage = 0.20m,
                    DiscountLabel = "20% OFF",
                    Description = "Maximum savings with annual billing",
                    IsPopular = false,
                    IsDefault = false,
                    ValidFrom = null,
                    ValidTo = null,
                    ApplicableRegions = "*",
                    ApplicableUserTypes = "*",
                    Currency = "ZAR",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.PaymentPlans.AddRangeAsync(paymentPlans);
            await context.SaveChangesAsync();

            Console.WriteLine($"Successfully seeded {paymentPlans.Count} payment plans.");
        }

        public static async Task SeedPaymentPlansForCurrencyAsync(PosDbContext context, string currency)
        {
            // Check if payment plans already exist for this currency
            if (await context.PaymentPlans.AnyAsync(p => p.Currency == currency))
            {
                Console.WriteLine($"Payment plans for {currency} already exist. Skipping seeding.");
                return;
            }

            Console.WriteLine($"Seeding payment plans for {currency}...");

            var paymentPlans = new List<PaymentPlan>
            {
                new PaymentPlan
                {
                    Name = "Monthly",
                    Period = "1 month",
                    DiscountPercentage = 0.0m,
                    DiscountLabel = null,
                    Description = "Pay monthly with full flexibility",
                    IsPopular = false,
                    IsDefault = true,
                    ValidFrom = null,
                    ValidTo = null,
                    ApplicableRegions = "*",
                    ApplicableUserTypes = "*",
                    Currency = currency,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new PaymentPlan
                {
                    Name = "Quarterly",
                    Period = "3 months",
                    DiscountPercentage = 0.10m,
                    DiscountLabel = "10% OFF",
                    Description = "Save 10% with quarterly billing",
                    IsPopular = false,
                    IsDefault = false,
                    ValidFrom = null,
                    ValidTo = null,
                    ApplicableRegions = "*",
                    ApplicableUserTypes = "*",
                    Currency = currency,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new PaymentPlan
                {
                    Name = "Semi-Annual",
                    Period = "6 months",
                    DiscountPercentage = 0.15m,
                    DiscountLabel = "15% OFF",
                    Description = "Save 15% with semi-annual billing",
                    IsPopular = true,
                    IsDefault = false,
                    ValidFrom = null,
                    ValidTo = null,
                    ApplicableRegions = "*",
                    ApplicableUserTypes = "*",
                    Currency = currency,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new PaymentPlan
                {
                    Name = "Annual",
                    Period = "12 months",
                    DiscountPercentage = 0.20m,
                    DiscountLabel = "20% OFF",
                    Description = "Maximum savings with annual billing",
                    IsPopular = false,
                    IsDefault = false,
                    ValidFrom = null,
                    ValidTo = null,
                    ApplicableRegions = "*",
                    ApplicableUserTypes = "*",
                    Currency = currency,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.PaymentPlans.AddRangeAsync(paymentPlans);
            await context.SaveChangesAsync();

            Console.WriteLine($"Successfully seeded {paymentPlans.Count} payment plans for {currency}.");
        }
    }
}
