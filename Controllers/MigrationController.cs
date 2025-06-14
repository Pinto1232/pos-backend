using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models;
using System.Text.Json;

namespace PosBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MigrationController : ControllerBase
    {
        private readonly PosDbContext _context;
        private readonly ILogger<MigrationController> _logger;

        public MigrationController(PosDbContext context, ILogger<MigrationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Migrates pricing data from legacy Price field to new PackagePrices table
        /// This fixes the issue where packages show $0.00 because the new pricing structure isn't populated
        /// </summary>
        [HttpPost("migrate-pricing-data")]
        public async Task<IActionResult> MigratePricingData()
        {
            try
            {
                _logger.LogInformation("Starting pricing data migration...");

                // Get all packages that need migration (packages without any prices in the new structure)
                var packages = await _context.PricingPackages
                    .Include(p => p.Prices)
                    .Where(p => !p.Prices.Any()) // Only migrate packages that don't have prices in the new structure
                    .ToListAsync();

                // Filter packages that have legacy pricing data to migrate
#pragma warning disable CS0618 // Type or member is obsolete - needed for migration
                packages = packages.Where(p => p.Price > 0 || !string.IsNullOrEmpty(p.MultiCurrencyPrices)).ToList();
#pragma warning restore CS0618

                if (!packages.Any())
                {
                    return Ok(new { message = "No packages found with legacy pricing data to migrate.", packagesProcessed = 0 });
                }

                var migrationResults = new List<object>();
                int totalMigrated = 0;

                foreach (var package in packages)
                {
#pragma warning disable CS0618 // Type or member is obsolete - needed for migration
                    var packageResult = new
                    {
                        PackageId = package.Id,
                        Title = package.Title,
                        Type = package.Type,
                        LegacyPrice = package.Price,
                        LegacyCurrency = package.Currency,
                        MultiCurrencyPrices = package.MultiCurrencyPrices,
                        ExistingPrices = package.Prices.Select(p => new { p.Currency, p.Price }).ToList(),
                        NewPricesAdded = new List<object>()
                    };
#pragma warning restore CS0618

                    _logger.LogInformation($"Processing package: {package.Title} (ID: {package.Id})");

#pragma warning disable CS0618 // Type or member is obsolete - needed for migration
                    // Migrate base USD price if not already exists
                    if (!package.Prices.Any(p => p.Currency == "USD"))
                    {
                        var usdPrice = new PackagePrice
                        {
                            PackageId = package.Id,
                            Currency = "USD",
                            Price = package.Price,
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        _context.PackagePrices.Add(usdPrice);
                        ((List<object>)packageResult.NewPricesAdded).Add(new { Currency = "USD", Price = package.Price });
                        _logger.LogInformation($"Added USD price: {package.Price:C}");
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
                                        var newPrice = new PackagePrice
                                        {
                                            PackageId = package.Id,
                                            Currency = currencyPrice.Key,
                                            Price = currencyPrice.Value,
                                            CreatedAt = DateTime.UtcNow
                                        };
                                        
                                        _context.PackagePrices.Add(newPrice);
                                        ((List<object>)packageResult.NewPricesAdded).Add(new { Currency = currencyPrice.Key, Price = currencyPrice.Value });
                                        _logger.LogInformation($"Added {currencyPrice.Key} price: {currencyPrice.Value:C}");
                                    }
                                }
                            }
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogWarning(ex, $"Failed to parse MultiCurrencyPrices for package {package.Id}: {package.MultiCurrencyPrices}");
                        }
                    }
#pragma warning restore CS0618

                    migrationResults.Add(packageResult);
                    totalMigrated++;
                }

                // Save all changes
                var savedChanges = await _context.SaveChangesAsync();
                _logger.LogInformation($"Migration completed. Processed {totalMigrated} packages, saved {savedChanges} changes.");

                return Ok(new 
                { 
                    message = "Pricing data migration completed successfully!",
                    packagesProcessed = totalMigrated,
                    changesSaved = savedChanges,
                    migrationResults = migrationResults
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during pricing data migration");
                return StatusCode(500, new { message = "Migration failed", error = ex.Message });
            }
        }

        /// <summary>
        /// Verifies the pricing data migration results
        /// </summary>
        [HttpGet("verify-pricing-migration")]
        public async Task<IActionResult> VerifyPricingMigration()
        {
            try
            {
                var packages = await _context.PricingPackages
                    .Include(p => p.Prices)
                    .ToListAsync();

#pragma warning disable CS0618 // Type or member is obsolete - needed for verification
                var results = packages.Select(p => new
                {
                    PackageId = p.Id,
                    Title = p.Title,
                    Type = p.Type,
                    LegacyPrice = p.Price,
                    LegacyCurrency = p.Currency,
                    NewPricesCount = p.Prices?.Count ?? 0,
                    NewPrices = p.Prices?.Select(pr => (object)new { pr.Currency, pr.Price }).ToList() ?? new List<object>(),
                    HasValidPrices = p.Prices?.Any() == true
                }).ToList();
#pragma warning restore CS0618

                var summary = new
                {
                    TotalPackages = packages.Count,
                    PackagesWithNewPrices = packages.Count(p => p.Prices?.Any() == true),
                    PackagesWithoutNewPrices = packages.Count(p => p.Prices?.Any() != true),
                    TotalPriceRecords = packages.SelectMany(p => p.Prices ?? new List<PackagePrice>()).Count()
                };

                return Ok(new
                {
                    summary = summary,
                    packageDetails = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying pricing migration");
                return StatusCode(500, new { message = "Verification failed", error = ex.Message });
            }
        }

        /// <summary>
        /// Shows current pricing data for debugging
        /// </summary>
        [HttpGet("debug-pricing-data")]
        public async Task<IActionResult> DebugPricingData()
        {
            try
            {
                var packages = await _context.PricingPackages
                    .Include(p => p.Prices)
                    .ToListAsync();

#pragma warning disable CS0618 
                var results = packages.Select(p => new
                {
                    PackageId = p.Id,
                    Title = p.Title,
                    Type = p.Type,
                    LegacyPrice = p.Price,
                    LegacyCurrency = p.Currency,
                    MultiCurrencyPrices = p.MultiCurrencyPrices,
                    NewPrices = p.Prices?.Select(pr => (object)new { 
                        Id = pr.Id,
                        Currency = pr.Currency, 
                        Price = pr.Price,
                        CreatedAt = pr.CreatedAt
                    }).ToList() ?? new List<object>(),
                    GetPriceUSD = p.GetPrice("USD"),
                    GetPriceEUR = p.GetPrice("EUR"),
                    GetPriceGBP = p.GetPrice("GBP")
                }).ToList();
#pragma warning restore CS0618

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error debugging pricing data");
                return StatusCode(500, new { message = "Debug failed", error = ex.Message });
            }
        }
    }
}