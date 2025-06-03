using Microsoft.EntityFrameworkCore;
using PosBackend.Models;

namespace PosBackend
{
    public static class UpdateBillingInfo
    {
        public static async Task UpdateBillingInfoAsync(string[] args)
        {
            var connectionString = "Host=localhost;Database=pos_db;Username=postgres;Password=admin123";

            var options = new DbContextOptionsBuilder<PosDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            using var context = new PosDbContext(options);

            try
            {
                Console.WriteLine("Updating billing information for existing packages...");

                // Update packages that have empty or null billing information
                var packagesToUpdate = await context.PricingPackages
                    .Where(p => string.IsNullOrEmpty(p.BillingInterval) || p.BillingIntervalCount == 0)
                    .ToListAsync();

                Console.WriteLine($"Found {packagesToUpdate.Count} packages to update.");

                foreach (var package in packagesToUpdate)
                {
                    package.IsSubscription = true;
                    package.BillingInterval = "month";
                    package.BillingIntervalCount = 1;

                    if (string.IsNullOrEmpty(package.StripeMultiCurrencyPriceIds))
                    {
                        package.StripeMultiCurrencyPriceIds = "{}";
                    }

                    Console.WriteLine($"Updated package: {package.Title} ({package.Type})");
                }

                await context.SaveChangesAsync();
                Console.WriteLine("Successfully updated all packages.");

                // Verify the updates
                var allPackages = await context.PricingPackages
                    .Select(p => new { p.Id, p.Title, p.Type, p.IsSubscription, p.BillingInterval, p.BillingIntervalCount })
                    .OrderBy(p => p.Id)
                    .ToListAsync();

                Console.WriteLine("\nCurrent package billing information:");
                Console.WriteLine("ID | Title | Type | IsSubscription | BillingInterval | BillingIntervalCount");
                Console.WriteLine("---|-------|------|----------------|-----------------|--------------------");

                foreach (var package in allPackages)
                {
                    Console.WriteLine($"{package.Id,2} | {package.Title,-15} | {package.Type,-15} | {package.IsSubscription,-14} | {package.BillingInterval,-15} | {package.BillingIntervalCount,19}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating billing information: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
