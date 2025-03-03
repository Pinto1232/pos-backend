using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PosBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PricingPackagesController : ControllerBase
    {
        private readonly PosDbContext _context;

        public PricingPackagesController(PosDbContext context)
        {
            _context = context;
        }

        // GET: api/PricingPackages (Paginated)
        [HttpGet]
        public async Task<ActionResult<object>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (_context.PricingPackages == null)
            {
                return NotFound("Pricing packages not found");
            }

            var totalItems = await _context.PricingPackages.CountAsync();

            // Materialize the data into memory first
            var packagesData = await _context.PricingPackages
                .OrderBy(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Then project into the DTO, retrieving shadow properties via the Entry API.
            var packages = packagesData.Select(p => new PricingPackageDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Icon = p.Icon,
                ExtraDescription = p.ExtraDescription,
                Price = p.Price,
                TestPeriodDays = p.TestPeriodDays,
                Type = p.Type,
                DescriptionList = p.Description.Split(';').ToList(),
                IsCustomizable = p.Type.ToLower() == "custom",
                // Retrieve the shadow properties from the DbContext's entry for p.
                Currency = _context.Entry(p).Property("Currency").CurrentValue as string ?? "",
                MultiCurrencyPrices = _context.Entry(p).Property("MultiCurrencyPrices").CurrentValue as string ?? ""
            }).ToList();

            return Ok(new
            {
                totalItems,
                data = packages
            });
        }

        // GET: api/PricingPackages/custom/features
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

        // POST: api/PricingPackages/custom/select
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

            // Clear previous selections
            package.SelectedFeatures?.Clear();
            package.SelectedAddOns?.Clear();
            package.SelectedUsageBasedPricing?.Clear();

            // Add new selections
            package.SelectedFeatures = request.SelectedFeatures
                .Select(f => new CustomPackageSelectedFeature 
                { 
                    PricingPackageId = package.Id, 
                    FeatureId = f 
                })
                .ToList();

            package.SelectedAddOns = request.SelectedAddOns
                .Select(a => new CustomPackageSelectedAddOn 
                { 
                    PricingPackageId = package.Id, 
                    AddOnId = a 
                })
                .ToList();

            package.SelectedUsageBasedPricing = request.UsageLimits
                .Select(u => new CustomPackageUsageBasedPricing 
                { 
                    PricingPackageId = package.Id, 
                    UsageBasedPricingId = u.Key, 
                    Quantity = u.Value 
                })
                .ToList();

            await _context.SaveChangesAsync();
            return Ok(new { message = "Custom package updated successfully" });
        }

        // POST: api/PricingPackages/custom/calculate-price
        [HttpPost("custom/calculate-price")]
        public async Task<ActionResult<object>> CalculateCustomPrice([FromBody] CustomPricingRequest request)
        {
            var package = await _context.PricingPackages
                .FirstOrDefaultAsync(p => p.Id == request.PackageId);
            
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

            return Ok(new
            {
                basePrice,
                totalPrice
            });
        }

        // DTO Classes

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
