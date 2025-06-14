using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PosBackend.Application.Services.Caching;
using PosBackend.Models;
using PosBackend.Services;
using PosBackend.Security;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using AppCacheKeys = PosBackend.Application.Services.Caching.CacheKeys;

namespace PosBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting(SecurityConstants.RateLimiting.AnonymousUserPolicy)]
    public class PricingPackagesController : PublicReadOnlyController
    {
        private readonly PosDbContext _context;
        private readonly GeoLocationService _geoService;
        private readonly ILogger<PricingPackagesController> _logger;
        private readonly ICacheService _cacheService;


        private readonly Dictionary<string, string> _countryToCurrency = new Dictionary<string, string>
        {
            { "US", "USD" },
            { "ZA", "ZAR" },
            { "GB", "GBP" },
            { "FR", "EUR" },

        };

        public PricingPackagesController(
            PosDbContext context,
            ILogger<PricingPackagesController> logger,
            ICacheService cacheService)
        {
            _context = context;
            _logger = logger;
            _cacheService = cacheService;

            string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "GeoLite2-Country.mmdb");
            try
            {
                var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                var geoLogger = loggerFactory.CreateLogger<GeoLocationService>();
                _geoService = new GeoLocationService(dbPath, cacheService, geoLogger);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize GeoLocationService with dbPath: {dbPath}", dbPath);
                var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                var geoLogger = loggerFactory.CreateLogger<GeoLocationService>();
                _geoService = new GeoLocationServiceFallback(cacheService, geoLogger);
            }
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (_context.PricingPackages == null)
                {
                    return NotFound("Pricing packages not found");
                }

                string cacheKey = AppCacheKeys.AllPackages + $":page:{pageNumber}:size:{pageSize}";

                return await _cacheService.GetOrSetAsync<ActionResult<object>>(cacheKey, async () =>
                {
                    _logger.LogDebug("Cache miss for pricing packages. Fetching from database.");

                    string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                    string countryCode = _geoService.GetCountryCode(ipAddress);

                    string userCurrency = _countryToCurrency.ContainsKey(countryCode)
                        ? _countryToCurrency[countryCode]
                        : "USD";

                    var totalItems = await _context.PricingPackages.CountAsync();

                    var packagesData = await _context.PricingPackages
                        .Include(p => p.Tier)
                        .Include(p => p.Prices)
                        .OrderBy(p => p.TierLevel)
                        .ThenBy(p => p.Id)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    var packages = packagesData.Select(p =>
                    {
                        var finalPrice = p.GetPrice(userCurrency);
                        if (finalPrice == 0)
                        {
                            finalPrice = p.GetPrice(); // fallback to default currency
                        }

                        return new PricingPackageDto
                        {
                            Id = p.Id,
                            Title = p.Title,
                            Description = p.Description,
                            Icon = p.Icon,
                            ExtraDescription = p.ExtraDescription,
                            Price = finalPrice,
                            TestPeriodDays = p.TestPeriodDays,
                            Type = p.Type,
                            DescriptionList = p.Description.Split(';').ToList(),
                            IsCustomizable = p.Type.ToLower() == "custom" || p.Type.ToLower() == "custom-pro",
                            Currency = p.GetPrice(userCurrency) > 0 ? userCurrency : p.GetPrimaryCurrency(),
                            MultiCurrencyPrices = System.Text.Json.JsonSerializer.Serialize(p.GetAllPrices()),
                            TierId = p.TierId,
                            TierLevel = p.TierLevel,
                            TierName = p.Tier?.Name,
                            TierDescription = p.Tier?.Description
                        };
                    }).ToList();

                    return Ok(new
                    {
                        totalItems,
                        data = packages
                    });
                });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetAll pricing packages");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PricingPackageDto>> GetById(int id)
        {
            try
            {
                string cacheKey = AppCacheKeys.Package(id);

                return await _cacheService.GetOrSetAsync<ActionResult<PricingPackageDto>>(cacheKey, async () =>
                {
                    _logger.LogDebug("Cache miss for pricing package ID: {Id}. Fetching from database.", id);

                    var package = await _context.PricingPackages
                        .Include(p => p.Tier)
                        .Include(p => p.Prices)
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (package == null)
                    {
                        return NotFound($"Pricing package with ID {id} not found");
                    }

                    string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                    string countryCode = _geoService.GetCountryCode(ipAddress);

                    string userCurrency = _countryToCurrency.ContainsKey(countryCode)
                        ? _countryToCurrency[countryCode]
                        : "USD";

                    decimal finalPrice = package.GetPrice(userCurrency);
                    if (finalPrice == 0)
                    {
                        finalPrice = package.GetPrice(); // fallback to default currency
                    }

                    return Ok(new PricingPackageDto
                    {
                        Id = package.Id,
                        Title = package.Title,
                        Description = package.Description,
                        Icon = package.Icon,
                        ExtraDescription = package.ExtraDescription,
                        Price = finalPrice,
                        TestPeriodDays = package.TestPeriodDays,
                        Type = package.Type,
                        DescriptionList = package.Description.Split(';').ToList(),
                        IsCustomizable = package.Type.ToLower() == "custom" || package.Type.ToLower() == "custom-pro",
                        Currency = package.GetPrice(userCurrency) > 0 ? userCurrency : package.GetPrimaryCurrency(),
                        MultiCurrencyPrices = System.Text.Json.JsonSerializer.Serialize(package.GetAllPrices()),
                        TierId = package.TierId,
                        TierLevel = package.TierLevel,
                        TierName = package.Tier?.Name,
                        TierDescription = package.Tier?.Description
                    });
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pricing package with ID {id}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("custom/features")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetCustomFeatures()
        {
            string cacheKey = "CustomFeatures";

            return await _cacheService.GetOrSetAsync<ActionResult<object>>(cacheKey, async () =>
            {
                _logger.LogDebug("Cache miss for custom features. Fetching from database.");

                var features = await _context.CoreFeatures.ToListAsync();
                var addOns = await _context.AddOns.ToListAsync();
                var usageBasedPricing = await _context.UsageBasedPricing.ToListAsync();

                var usageBasedPricingWithDefault = usageBasedPricing.Select(u => new
                {
                    u.Id,
                    u.FeatureId,
                    u.Name,
                    u.Unit,
                    u.MinValue,
                    u.MaxValue,
                    u.PricePerUnit,
                    defaultValue = u.MinValue
                }).ToList();

                return Ok(new
                {
                    coreFeatures = features,
                    addOns = addOns,
                    usageBasedPricing = usageBasedPricingWithDefault
                });
            });
        }

        [HttpPost("custom/select")]
        public async Task<IActionResult> SelectCustomPackage([FromBody] CustomSelectionRequest request)
        {
            var package = await _context.PricingPackages
                .Include(p => p.SelectedFeatures)
                .Include(p => p.SelectedAddOns)
                .Include(p => p.SelectedUsageBasedPricing)
                .FirstOrDefaultAsync(p => p.Id == request.PackageId && (p.Type.ToLower() == "custom" || p.Type.ToLower() == "custom-pro"));

            if (package == null)
                return NotFound("Custom package not found");

            package.SelectedFeatures?.Clear();
            package.SelectedAddOns?.Clear();
            package.SelectedUsageBasedPricing?.Clear();

            package.SelectedFeatures = request.SelectedFeatures
                .Select(f => new CustomPackageSelectedFeature
                {
                    PricingPackageId = package.Id,
                    FeatureId = f
                }).ToList();

            package.SelectedAddOns = request.SelectedAddOns
                .Select(a => new CustomPackageSelectedAddOn
                {
                    PricingPackageId = package.Id,
                    AddOnId = a
                }).ToList();

            package.SelectedUsageBasedPricing = request.UsageLimits
                .Select(u => new CustomPackageUsageBasedPricing
                {
                    PricingPackageId = package.Id,
                    UsageBasedPricingId = u.Key,
                    Quantity = u.Value
                }).ToList();

            await _context.SaveChangesAsync();

            await _cacheService.RemoveAsync(AppCacheKeys.Package(request.PackageId));
            await _cacheService.RemoveByPrefixAsync(AppCacheKeys.PackagePrefix);

            return Ok(new { message = "Custom package updated successfully" });
        }

        [HttpPost("custom/calculate-price")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> CalculateCustomPrice([FromBody] CustomPricingRequest request)
        {
            var package = await _context.PricingPackages
                .Include(p => p.Prices)
                .FirstOrDefaultAsync(p => p.Id == request.PackageId);
            if (package == null)
                return BadRequest("Invalid package");

            decimal basePrice = package.GetPrice();
            decimal totalPrice = basePrice;

            var selectedFeatures = await _context.CoreFeatures
                .Where(f => request.SelectedFeatures.Contains(f.Id))
                .ToListAsync();

            var selectedAddOns = await _context.AddOns
                .Where(a => request.SelectedAddOns.Contains(a.Id))
                .ToListAsync();

            var selectedUsage = await _context.UsageBasedPricing
                .Where(u => request.UsageLimits.Keys.Contains(u.Id))
                .ToListAsync();

            totalPrice += selectedFeatures.Sum(f => f.BasePrice);
            totalPrice += selectedAddOns.Sum(a => a.Price);

            foreach (var usage in selectedUsage)
            {
                if (request.UsageLimits.TryGetValue(usage.Id, out var quantity))
                {
                    totalPrice += quantity * usage.PricePerUnit;
                }
            }

            return Ok(new { basePrice, totalPrice });
        }

        /// <summary>
        /// Creates a new pricing package
        /// </summary>
        /// <param name="packageDto">The pricing package data</param>
        /// <returns>The newly created pricing package</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<PricingPackageDto>> CreatePricingPackage([FromBody] CreatePricingPackageDto packageDto)
        {
            try
            {
                // Check if a package with the same type already exists
                var existingPackage = await _context.PricingPackages
                    .FirstOrDefaultAsync(p => p.Type.ToLower() == packageDto.Type.ToLower());

                if (existingPackage != null)
                {
                    return Conflict($"A pricing package with type '{packageDto.Type}' already exists");
                }

                // Create new pricing package
                var newPackage = new PricingPackage
                {
                    Title = packageDto.Title,
                    Description = packageDto.Description,
                    Icon = packageDto.Icon,
                    ExtraDescription = packageDto.ExtraDescription,
                    TestPeriodDays = packageDto.TestPeriodDays,
                    Type = packageDto.Type
                };

                _context.PricingPackages.Add(newPackage);
                await _context.SaveChangesAsync();

                // Add price using the new structure
                newPackage.SetPrice(packageDto.Price, packageDto.Currency ?? "USD");
                await _context.SaveChangesAsync();

                // Invalidate cache for all packages
                await _cacheService.RemoveByPrefixAsync(AppCacheKeys.PackagePrefix);

                // Return the created package
                return CreatedAtAction(
                    nameof(GetAll),
                    new { id = newPackage.Id },
                    new PricingPackageDto
                    {
                        Id = newPackage.Id,
                        Title = newPackage.Title,
                        Description = newPackage.Description,
                        Icon = newPackage.Icon,
                        ExtraDescription = newPackage.ExtraDescription,
                        Price = newPackage.GetPrice(),
                        TestPeriodDays = newPackage.TestPeriodDays,
                        Type = newPackage.Type,
                        DescriptionList = newPackage.Description.Split(';').ToList(),
                        IsCustomizable = newPackage.Type.ToLower() == "custom" || newPackage.Type.ToLower() == "custom-pro",
                        Currency = newPackage.GetPrimaryCurrency(),
                        MultiCurrencyPrices = System.Text.Json.JsonSerializer.Serialize(newPackage.GetAllPrices())
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pricing package");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing pricing package
        /// </summary>
        /// <param name="id">The ID of the pricing package to update</param>
        /// <param name="packageDto">The updated pricing package data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdatePricingPackage(int id, [FromBody] CreatePricingPackageDto packageDto)
        {
            try
            {
                var package = await _context.PricingPackages
                    .Include(p => p.Prices)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (package == null)
                {
                    return NotFound($"Pricing package with ID {id} not found");
                }

                // Check if trying to change the type to one that already exists
                if (package.Type.ToLower() != packageDto.Type.ToLower())
                {
                    var existingPackage = await _context.PricingPackages
                        .FirstOrDefaultAsync(p => p.Type.ToLower() == packageDto.Type.ToLower() && p.Id != id);

                    if (existingPackage != null)
                    {
                        return Conflict($"A pricing package with type '{packageDto.Type}' already exists");
                    }
                }

                // Update package properties
                package.Title = packageDto.Title;
                package.Description = packageDto.Description;
                package.Icon = packageDto.Icon;
                package.ExtraDescription = packageDto.ExtraDescription;
                package.TestPeriodDays = packageDto.TestPeriodDays;
                package.Type = packageDto.Type;
                
                // Update price using the new structure
                package.SetPrice(packageDto.Price, packageDto.Currency ?? "USD");

                await _context.SaveChangesAsync();

                // Invalidate cache for this package and all packages
                await _cacheService.RemoveAsync(AppCacheKeys.Package(id));
                await _cacheService.RemoveByPrefixAsync(AppCacheKeys.PackagePrefix);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating pricing package with ID {id}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a pricing package
        /// </summary>
        /// <param name="id">The ID of the pricing package to delete</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeletePricingPackage(int id)
        {
            try
            {
                var package = await _context.PricingPackages
                    .Include(p => p.Prices)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (package == null)
                {
                    return NotFound($"Pricing package with ID {id} not found");
                }

                // Check if this is a default package type that shouldn't be deleted
                var defaultTypes = new[] { "starter", "growth", "premium", "enterprise", "custom" };
                if (defaultTypes.Contains(package.Type.ToLower()))
                {
                    return BadRequest($"Cannot delete default package type '{package.Type}'. Consider updating it instead.");
                }

                _context.PricingPackages.Remove(package);
                await _context.SaveChangesAsync();

                // Invalidate cache for this package and all packages
                await _cacheService.RemoveAsync(AppCacheKeys.Package(id));
                await _cacheService.RemoveByPrefixAsync(AppCacheKeys.PackagePrefix);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting pricing package with ID {id}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        public class PricingPackageDto
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Icon { get; set; } = string.Empty;
            public string ExtraDescription { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int TestPeriodDays { get; set; }
            public string Type { get; set; } = string.Empty;
            public List<string> DescriptionList { get; set; } = new List<string>();
            public bool IsCustomizable { get; set; }
            public string Currency { get; set; } = string.Empty;
            public string MultiCurrencyPrices { get; set; } = string.Empty;
            
            // Tier information
            public int? TierId { get; set; }
            public int TierLevel { get; set; }
            public string? TierName { get; set; }
            public string? TierDescription { get; set; }
        }

        public class CreatePricingPackageDto
        {
            [Required]
            public string Title { get; set; } = string.Empty;

            [Required]
            public string Description { get; set; } = string.Empty;

            [Required]
            public string Icon { get; set; } = string.Empty;

            public string ExtraDescription { get; set; } = string.Empty;

            [Required]
            public decimal Price { get; set; }

            public int TestPeriodDays { get; set; } = 14;

            [Required]
            public string Type { get; set; } = string.Empty;

            public string Currency { get; set; } = "USD";

            public string MultiCurrencyPrices { get; set; } = "{}";
        }

        public class CustomSelectionRequest
        {
            public int PackageId { get; set; }
            public List<int> SelectedFeatures { get; set; } = new List<int>();
            public List<int> SelectedAddOns { get; set; } = new List<int>();
            public Dictionary<int, int> UsageLimits { get; set; } = new Dictionary<int, int>();
        }

        public class CustomPricingRequest
        {
            public int PackageId { get; set; }
            public List<int> SelectedFeatures { get; set; } = new List<int>();
            public List<int> SelectedAddOns { get; set; } = new List<int>();
            public Dictionary<int, int> UsageLimits { get; set; } = new Dictionary<int, int>();
        }
    }
}