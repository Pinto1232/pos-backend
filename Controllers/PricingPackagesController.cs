using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models;
using PosBackend.Services;

namespace PosBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PricingPackagesController : ControllerBase
    {
        private readonly PosDbContext _context;
        private readonly GeoLocationService _geoService;
        private readonly ILogger<PricingPackagesController> _logger;


        private readonly Dictionary<string, string> _countryToCurrency = new Dictionary<string, string>
        {
            { "US", "USD" },
            { "ZA", "ZAR" },
            { "GB", "GBP" },
            { "FR", "EUR" },

        };

        public PricingPackagesController(PosDbContext context, ILogger<PricingPackagesController> logger)
        {
            _context = context;
            _logger = logger;
            string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "GeoLite2-Country.mmdb");
            try
            {
                _geoService = new GeoLocationService(dbPath);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize GeoLocationService with dbPath: {dbPath}", dbPath);
                _geoService = new GeoLocationServiceFallback();
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

                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                string countryCode = _geoService.GetCountryCode(ipAddress);

                string userCurrency = _countryToCurrency.ContainsKey(countryCode)
                    ? _countryToCurrency[countryCode]
                    : "USD";

                var totalItems = await _context.PricingPackages.CountAsync();

                var packagesData = await _context.PricingPackages
                    .OrderBy(p => p.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var packages = packagesData.Select(p =>
                {
                    var multiPrices = new Dictionary<string, decimal>();
                    try
                    {
                        multiPrices = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, decimal>>(p.MultiCurrencyPrices)
                            ?? new Dictionary<string, decimal>();
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, "Failed to parse MultiCurrencyPrices for package id {PackageId}", p.Id);
                    }

                    var finalPrice = p.Price;
                    if (multiPrices.TryGetValue(userCurrency, out decimal currencyPrice))
                    {
                        finalPrice = currencyPrice;
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
                        IsCustomizable = p.Type.ToLower() == "custom",
                        Currency = userCurrency,
                        MultiCurrencyPrices = p.MultiCurrencyPrices
                    };
                }).ToList();

                return Ok(new
                {
                    totalItems,
                    data = packages
                });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetAll pricing packages");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("custom/features")]
        public async Task<ActionResult<object>> GetCustomFeatures()
        {
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
        }

        [HttpPost("custom/select")]
        public async Task<IActionResult> SelectCustomPackage([FromBody] CustomSelectionRequest request)
        {
            var package = await _context.PricingPackages
                .Include(p => p.SelectedFeatures)
                .Include(p => p.SelectedAddOns)
                .Include(p => p.SelectedUsageBasedPricing)
                .FirstOrDefaultAsync(p => p.Id == request.PackageId && p.Type.ToLower() == "custom");

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
            return Ok(new { message = "Custom package updated successfully" });
        }

        [HttpPost("custom/calculate-price")]
        public async Task<ActionResult<object>> CalculateCustomPrice([FromBody] CustomPricingRequest request)
        {
            var package = await _context.PricingPackages.FirstOrDefaultAsync(p => p.Id == request.PackageId);
            if (package == null)
                return BadRequest("Invalid package");

            decimal basePrice = package.Price;
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