using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PosBackend.Application.Services.Caching;
using PosBackend.Models;
using PosBackend.Models.DTOs;
using PosBackend.Services.Interfaces;

namespace PosBackend.Services
{
    public class PricingService : IPricingService
    {
        private readonly PosDbContext _context;
        private readonly ICurrencyService _currencyService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<PricingService> _logger;
        private readonly PricingOptions _options;

        public PricingService(
            PosDbContext context,
            ICurrencyService currencyService,
            ICacheService cacheService,
            ILogger<PricingService> logger,
            IOptions<PricingOptions> options)
        {
            _context = context;
            _currencyService = currencyService;
            _cacheService = cacheService;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<PackagePrice?> GetPackagePriceAsync(int packageId, string currency)
        {
            try
            {
                var cacheKey = $"package_price_{packageId}_{currency}";
                var cachedPrice = await _cacheService.GetAsync<PackagePrice>(cacheKey);
                if (cachedPrice != null)
                {
                    return cachedPrice;
                }

                // Try to get exact currency price
                var exactPrice = await _context.PackagePrices
                    .Where(p => p.PackageId == packageId && p.Currency == currency)
                    .Where(p => p.ValidUntil == null || p.ValidUntil > DateTime.UtcNow)
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefaultAsync();

                if (exactPrice != null)
                {
                    await _cacheService.SetAsync(cacheKey, exactPrice, null, TimeSpan.FromMinutes(30));
                    return exactPrice;
                }

                // Try to get USD price and convert
                var usdPrice = await _context.PackagePrices
                    .Where(p => p.PackageId == packageId && p.Currency == "USD")
                    .Where(p => p.ValidUntil == null || p.ValidUntil > DateTime.UtcNow)
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefaultAsync();

                if (usdPrice != null && currency != "USD")
                {
                    var convertedAmount = await _currencyService.ConvertAsync(usdPrice.Price, "USD", currency);
                    var convertedPrice = new PackagePrice
                    {
                        PackageId = packageId,
                        Currency = currency,
                        Price = convertedAmount,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _cacheService.SetAsync(cacheKey, convertedPrice, null, TimeSpan.FromMinutes(15));
                    return convertedPrice;
                }

                // Fallback to legacy Price field with conversion
                var package = await _context.PricingPackages
                    .Include(p => p.Prices)
                    .FirstOrDefaultAsync(p => p.Id == packageId);

                if (package != null && package.GetPrice() > 0)
                {
                    var legacyCurrency = package.GetPrimaryCurrency();
                    var convertedAmount = await _currencyService.ConvertAsync(package.GetPrice(), legacyCurrency, currency);
                    
                    var legacyPrice = new PackagePrice
                    {
                        PackageId = packageId,
                        Currency = currency,
                        Price = convertedAmount,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _cacheService.SetAsync(cacheKey, legacyPrice, null, TimeSpan.FromMinutes(15));
                    return legacyPrice;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting package price for package {PackageId} in {Currency}", packageId, currency);
                return null;
            }
        }

        public async Task<IEnumerable<PackagePrice>> GetPackagePricesAsync(int packageId)
        {
            try
            {
                var cacheKey = $"package_prices_{packageId}";
                var cachedPrices = await _cacheService.GetAsync<List<PackagePrice>>(cacheKey);
                if (cachedPrices != null)
                {
                    return cachedPrices;
                }

                var prices = await _context.PackagePrices
                    .Where(p => p.PackageId == packageId)
                    .Where(p => p.ValidUntil == null || p.ValidUntil > DateTime.UtcNow)
                    .OrderBy(p => p.Currency)
                    .ThenByDescending(p => p.CreatedAt)
                    .ToListAsync();

                await _cacheService.SetAsync(cacheKey, prices, null, TimeSpan.FromHours(1));
                return prices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting package prices for package {PackageId}", packageId);
                return new List<PackagePrice>();
            }
        }

        public async Task<object> GetPackagesWithPricingAsync(int pageNumber, int pageSize, string currency)
        {
            try
            {
                var cacheKey = $"packages_with_pricing_{pageNumber}_{pageSize}_{currency}";
                var cached = await _cacheService.GetAsync<object>(cacheKey);
                if (cached != null)
                {
                    return cached;
                }

                var totalItems = await _context.PricingPackages.CountAsync();

                var packages = await _context.PricingPackages
                    .Include(p => p.Tier)
                    .Include(p => p.Prices.Where(pr => pr.Currency == currency && 
                        (pr.ValidUntil == null || pr.ValidUntil > DateTime.UtcNow)))
                    .OrderBy(p => p.TierLevel)
                    .ThenBy(p => p.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var packageDtos = new List<PricingPackageDto>();

                foreach (var package in packages)
                {
                    var price = await GetPackagePriceAsync(package.Id, currency);
                    
                    packageDtos.Add(new PricingPackageDto
                    {
                        Id = package.Id,
                        Title = package.Title,
                        Description = package.Description,
                        Icon = package.Icon,
                        ExtraDescription = package.ExtraDescription,
                        Price = price?.Price ?? 0,
                        TestPeriodDays = package.TestPeriodDays,
                        Type = package.Type,
                        DescriptionList = package.Description.Split(';').ToList(),
                        IsCustomizable = package.Type.ToLower() == "custom" || package.Type.ToLower() == "custom-pro",
                        Currency = currency,
                        MultiCurrencyPrices = await GetMultiCurrencyPricesJsonAsync(package.Id),
                        TierId = package.TierId,
                        TierLevel = package.TierLevel,
                        TierName = package.Tier?.Name,
                        TierDescription = package.Tier?.Description
                    });
                }

                var result = new
                {
                    totalItems,
                    data = packageDtos
                };

                await _cacheService.SetAsync(cacheKey, result, null, TimeSpan.FromMinutes(30));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting packages with pricing for currency {Currency}", currency);
                throw;
            }
        }

        public async Task<PricingPackageDto?> GetPackageByIdAsync(int packageId, string currency)
        {
            try
            {
                var cacheKey = $"package_{packageId}_{currency}";
                var cached = await _cacheService.GetAsync<PricingPackageDto>(cacheKey);
                if (cached != null)
                {
                    return cached;
                }

                var package = await _context.PricingPackages
                    .Include(p => p.Tier)
                    .FirstOrDefaultAsync(p => p.Id == packageId);

                if (package == null)
                {
                    return null;
                }

                var price = await GetPackagePriceAsync(packageId, currency);

                var dto = new PricingPackageDto
                {
                    Id = package.Id,
                    Title = package.Title,
                    Description = package.Description,
                    Icon = package.Icon,
                    ExtraDescription = package.ExtraDescription,
                    Price = price?.Price ?? 0,
                    TestPeriodDays = package.TestPeriodDays,
                    Type = package.Type,
                    DescriptionList = package.Description.Split(';').ToList(),
                    IsCustomizable = package.Type.ToLower() == "custom" || package.Type.ToLower() == "custom-pro",
                    Currency = currency,
                    MultiCurrencyPrices = await GetMultiCurrencyPricesJsonAsync(packageId),
                    TierId = package.TierId,
                    TierLevel = package.TierLevel,
                    TierName = package.Tier?.Name,
                    TierDescription = package.Tier?.Description
                };

                await _cacheService.SetAsync(cacheKey, dto, null, TimeSpan.FromMinutes(30));
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting package {PackageId} for currency {Currency}", packageId, currency);
                return null;
            }
        }

        public async Task<decimal> CalculateCustomPackagePriceAsync(CustomPricingRequest request, string currency)
        {
            try
            {
                var basePackage = await GetPackagePriceAsync(request.PackageId, currency);
                if (basePackage == null)
                {
                    throw new InvalidOperationException($"Package {request.PackageId} not found");
                }

                decimal totalPrice = basePackage.Price;

                // Add core features
                if (request.SelectedFeatures?.Any() == true)
                {
                    var features = await _context.CoreFeatures
                        .Where(f => request.SelectedFeatures.Contains(f.Id))
                        .ToListAsync();

                    foreach (var feature in features)
                    {
                        var convertedPrice = await _currencyService.ConvertAsync(feature.BasePrice, "USD", currency);
                        totalPrice += convertedPrice;
                    }
                }

                // Add add-ons
                if (request.SelectedAddOns?.Any() == true)
                {
                    var addOns = await _context.AddOns
                        .Where(a => request.SelectedAddOns.Contains(a.Id))
                        .ToListAsync();

                    foreach (var addOn in addOns)
                    {
                        var addOnCurrency = string.IsNullOrEmpty(addOn.Currency) ? "USD" : addOn.Currency;
                        var convertedPrice = await _currencyService.ConvertAsync(addOn.Price, addOnCurrency, currency);
                        totalPrice += convertedPrice;
                    }
                }

                // Add usage-based pricing
                if (request.UsageLimits?.Any() == true)
                {
                    var usageItems = await _context.UsageBasedPricing
                        .Where(u => request.UsageLimits.Keys.Contains(u.Id))
                        .ToListAsync();

                    foreach (var usage in usageItems)
                    {
                        if (request.UsageLimits.TryGetValue(usage.Id, out var quantity))
                        {
                            var convertedPricePerUnit = await _currencyService.ConvertAsync(usage.PricePerUnit, "USD", currency);
                            totalPrice += quantity * convertedPricePerUnit;
                        }
                    }
                }

                return Math.Round(totalPrice, GetCurrencyDecimalPlaces(currency));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating custom package price for package {PackageId} in {Currency}", 
                    request.PackageId, currency);
                throw;
            }
        }

        public async Task<PackagePrice> SetPackagePriceAsync(int packageId, string currency, decimal price, DateTime? validUntil = null)
        {
            try
            {
                // Validate package exists
                var packageExists = await _context.PricingPackages.AnyAsync(p => p.Id == packageId);
                if (!packageExists)
                {
                    throw new ArgumentException($"Package with ID {packageId} does not exist");
                }

                // Validate currency
                if (!_currencyService.IsCurrencySupported(currency))
                {
                    throw new ArgumentException($"Currency {currency} is not supported");
                }

                var packagePrice = new PackagePrice
                {
                    PackageId = packageId,
                    Currency = currency,
                    Price = Math.Round(price, GetCurrencyDecimalPlaces(currency)),
                    CreatedAt = DateTime.UtcNow,
                    ValidUntil = validUntil
                };

                _context.PackagePrices.Add(packagePrice);
                await _context.SaveChangesAsync();

                // Clear related caches
                await _cacheService.RemoveAsync($"package_price_{packageId}_{currency}");
                await _cacheService.RemoveAsync($"package_prices_{packageId}");
                await _cacheService.RemoveByPrefixAsync($"packages_with_pricing_");
                await _cacheService.RemoveByPrefixAsync($"package_{packageId}_");

                _logger.LogInformation("Set price {Price} {Currency} for package {PackageId}", price, currency, packageId);
                return packagePrice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting package price for package {PackageId}", packageId);
                throw;
            }
        }

        private async Task<string> GetMultiCurrencyPricesJsonAsync(int packageId)
        {
            try
            {
                var prices = await GetPackagePricesAsync(packageId);
                var priceDict = prices.ToDictionary(p => p.Currency, p => p.Price);
                return System.Text.Json.JsonSerializer.Serialize(priceDict);
            }
            catch
            {
                return "{}";
            }
        }

        private int GetCurrencyDecimalPlaces(string currency)
        {
            return currency switch
            {
                "JPY" => 0,
                "KRW" => 0,
                "VND" => 0,
                _ => 2
            };
        }
    }
}