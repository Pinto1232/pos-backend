using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PosBackend.Models;
using System.Text.Json;

namespace PosBackend.Scripts
{
    public class MigratePricingData
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== POS Pricing Data Migration Tool ===");
            Console.WriteLine("This tool migrates pricing data from legacy fields to the new PackagePrices table.");
            Console.WriteLine();

            // Create host and get configuration
            var host = CreateHostBuilder(args).Build();
            var logger = host.Services.GetRequiredService<ILogger<MigratePricingData>>();
            var context = host.Services.GetRequiredService<PosDbContext>();

            try
            {
                Console.WriteLine("1. Checking current pricing data...");
                await DisplayCurrentPricingData(context, logger);

                Console.WriteLine("2. Migrating pricing data...");
                await MigratePricingDataToNewStructure(context, logger);

                Console.WriteLine("3. Verifying migration results...");
                await VerifyMigrationResults(context, logger);

                Console.WriteLine("✅ Migration completed successfully!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Migration failed");
                Console.WriteLine($"❌ Migration failed: {ex.Message}");
                Console.WriteLine($"Full error: {ex}");
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Add Entity Framework
                    services.AddDbContext<PosDbContext>(options =>
                    {
                        var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
                        if (connectionString == "InMemory")
                        {
                            options.UseInMemoryDatabase("PosDatabase");
                        }
                        else
                        {
                            options.UseNpgsql(connectionString);
                        }
                    });
                });

        private static async Task DisplayCurrentPricingData(PosDbContext context, ILogger logger)
        {
            var packages = await context.PricingPackages
                .Include(p => p.Prices)
                .ToListAsync();

            Console.WriteLine($"Found {packages.Count} pricing packages:");
            foreach (var package in packages)
            {
                Console.WriteLine($"  - {package.Title} (ID: {package.Id})");
#pragma warning disable CS0618 // Type or member is obsolete - needed for migration
                Console.WriteLine($"    Legacy Price: {package.Price:C} {package.Currency}");
                Console.WriteLine($"    Multi-Currency Prices: {package.MultiCurrencyPrices}");
#pragma warning restore CS0618
                Console.WriteLine($"    New Prices Count: {package.Prices?.Count ?? 0}");
                
                if (package.Prices?.Any() == true)
                {
                    foreach (var price in package.Prices)
                    {
                        Console.WriteLine($"      {price.Currency}: {price.Price:C}");
                    }
                }
                Console.WriteLine();
            }
        }

        private static async Task MigratePricingDataToNewStructure(PosDbContext context, ILogger logger)
        {
            // Get all packages that need migration (packages without any prices in the new structure)
            var packages = await context.PricingPackages
                .Include(p => p.Prices)
                .Where(p => !p.Prices.Any()) // Only migrate packages that don't have prices in the new structure
                .ToListAsync();

            // Filter packages that have legacy pricing data to migrate
#pragma warning disable CS0618 // Type or member is obsolete - needed for migration
            packages = packages.Where(p => p.Price > 0 || !string.IsNullOrEmpty(p.MultiCurrencyPrices)).ToList();
#pragma warning restore CS0618

            int migratedCount = 0;

            foreach (var package in packages)
            {
                logger.LogInformation($"Migrating package: {package.Title} (ID: {package.Id})");

#pragma warning disable CS0618 // Type or member is obsolete - needed for migration
                // Migrate base USD price if not already exists
                if (!package.Prices.Any(p => p.Currency == "USD"))
                {
                    package.Prices.Add(new PackagePrice
                    {
                        PackageId = package.Id,
                        Currency = "USD",
                        Price = package.Price,
                        CreatedAt = DateTime.UtcNow
                    });
                    logger.LogInformation($"Added USD price: {package.Price:C}");
                }

                // Migrate multi-currency prices
                if (!string.IsNullOrEmpty(package.MultiCurrencyPrices) && 
                    package.MultiCurrencyPrices != "{}")
                {
                    try
                    {
                        var multiCurrencyPrices = JsonSerializer.Deserialize<Dictionary<string, decimal>>(package.MultiCurrencyPrices);
                        
                        if (multiCurrencyPrices != null)
                        {
                            foreach (var currencyPrice in multiCurrencyPrices)
                            {
                                if (!package.Prices.Any(p => p.Currency == currencyPrice.Key))
                                {
                                    package.Prices.Add(new PackagePrice
                                    {
                                        PackageId = package.Id,
                                        Currency = currencyPrice.Key,
                                        Price = currencyPrice.Value,
                                        CreatedAt = DateTime.UtcNow
                                    });
                                    logger.LogInformation($"Added {currencyPrice.Key} price: {currencyPrice.Value:C}");
                                }
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        logger.LogWarning(ex, $"Failed to parse MultiCurrencyPrices for package {package.Id}: {package.MultiCurrencyPrices}");
                    }
                }
#pragma warning restore CS0618

                migratedCount++;
            }

            // Save all changes
            var savedChanges = await context.SaveChangesAsync();
            logger.LogInformation($"Migration completed. Migrated {migratedCount} packages, saved {savedChanges} changes.");
            Console.WriteLine($"✅ Migrated {migratedCount} packages successfully.");
        }

        private static async Task VerifyMigrationResults(PosDbContext context, ILogger logger)
        {
            var packages = await context.PricingPackages
                .Include(p => p.Prices)
                .ToListAsync();

            Console.WriteLine("Migration Results:");
            Console.WriteLine("==================");

            foreach (var package in packages)
            {
                Console.WriteLine($"{package.Title} (Type: {package.Type}):");
#pragma warning disable CS0618 // Type or member is obsolete - needed for verification
                Console.WriteLine($"  Legacy Price: {package.Price:C}");
#pragma warning restore CS0618
                
                if (package.Prices?.Any() == true)
                {
                    Console.WriteLine("  New Prices:");
                    foreach (var price in package.Prices.OrderBy(p => p.Currency))
                    {
                        Console.WriteLine($"    {price.Currency}: {price.Price:C}");
                    }
                }
                else
                {
                    Console.WriteLine("  ⚠️  No prices in new structure!");
                }
                Console.WriteLine();
            }

            // Summary
            var totalPackages = packages.Count;
            var packagesWithNewPrices = packages.Count(p => p.Prices?.Any() == true);
            var totalNewPrices = packages.SelectMany(p => p.Prices ?? new List<PackagePrice>()).Count();

            Console.WriteLine("Summary:");
            Console.WriteLine($"  Total packages: {totalPackages}");
            Console.WriteLine($"  Packages with new prices: {packagesWithNewPrices}");
            Console.WriteLine($"  Total price records: {totalNewPrices}");
            
            if (packagesWithNewPrices == totalPackages && totalNewPrices > 0)
            {
                Console.WriteLine("✅ All packages have been successfully migrated!");
            }
            else
            {
                Console.WriteLine("⚠️  Some packages may not have been migrated properly.");
            }
        }
    }
}