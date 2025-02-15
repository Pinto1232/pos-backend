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

        // GET: api/PricingPackages - Returns paginated pricing packages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PricingPackage>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (_context.PricingPackages == null)
            {
                return NotFound("No PricingPackages table found.");
            }

            var totalItems = await _context.PricingPackages.CountAsync();
            var packages = await _context.PricingPackages
                .OrderBy(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalItems = totalItems,
                Data = packages
            });
        }

        // GET: api/PricingPackages/{id} - Returns a single package, with customization fields if applicable
        [HttpGet("{id}")]
        public async Task<ActionResult<PricingPackage>> GetById(int id)
        {
            var package = await _context.PricingPackages.FindAsync(id);
            if (package == null)
                return NotFound("Package not found");

            if (package.Type.ToLower() == "custom")
            {
                package.CoreFeatures = new List<Feature>
                {
                    new Feature { Id = 101, Name = "Inventory Management", Description = "Track and manage your inventory in real-time.", BasePrice = 10.00m, IsRequired = true },
                    new Feature { Id = 102, Name = "Sales Reporting", Description = "Generate detailed reports on sales and revenue.", BasePrice = 8.00m, IsRequired = false },
                    new Feature { Id = 103, Name = "Multi-Location Support", Description = "Manage multiple store locations from one dashboard.", BasePrice = 12.00m, IsRequired = false }
                };

                package.AddOns = new List<AddOn>
                {
                    new AddOn { Id = 201, Name = "Premium Support", Description = "24/7 priority support via chat and email.", Price = 5.00m },
                    new AddOn { Id = 202, Name = "Custom Branding", Description = "Add your own logo and color scheme to the POS.", Price = 7.00m, Dependencies = new List<int> { 101 } }
                };

                package.UsageBasedPricingOptions = new List<UsageBasedPricing>
                {
                    new UsageBasedPricing { FeatureId = 101, Name = "API Calls", Unit = "requests", MinValue = 1000, MaxValue = 100000, PricePerUnit = 0.01m },
                    new UsageBasedPricing { FeatureId = 102, Name = "User Licenses", Unit = "users", MinValue = 1, MaxValue = 50, PricePerUnit = 5.00m }
                };
            }

            return Ok(package);
        }

        // POST: api/PricingPackages/custom/calculate-price - Dynamically calculates custom package pricing
        [HttpPost("custom/calculate-price")]
        public ActionResult<object> CalculateCustomPrice([FromBody] CustomPricingRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request data.");

            decimal basePrice = 99.99m;
            decimal totalPrice = basePrice;

            totalPrice += request.SelectedFeatures
                .Where(f => CustomPackageFeatures.ContainsKey(f))
                .Sum(f => CustomPackageFeatures[f].BasePrice);

            totalPrice += request.SelectedAddOns
                .Where(a => CustomPackageAddOns.ContainsKey(a))
                .Sum(a => CustomPackageAddOns[a].Price);

            foreach (var usage in request.UsageLimits)
            {
                if (CustomPackageUsagePricing.ContainsKey(usage.Key))
                {
                    var pricing = CustomPackageUsagePricing[usage.Key];
                    totalPrice += usage.Value * pricing.PricePerUnit;
                }
            }

            return Ok(new
            {
                BasePrice = basePrice,
                TotalPrice = totalPrice
            });
        }

        // GET: api/PricingPackages/public - Open endpoint for testing
        [HttpGet("public")]
        [AllowAnonymous]
        public ActionResult<string> PublicEndpoint()
        {
            return "This endpoint does not require a token!";
        }

        private static readonly Dictionary<int, Feature> CustomPackageFeatures = new()
        {
            { 101, new Feature { Id = 101, Name = "Inventory Management", Description = "Track and manage your inventory in real-time.", BasePrice = 10.00m, IsRequired = true } },
            { 102, new Feature { Id = 102, Name = "Sales Reporting", Description = "Generate detailed reports on sales and revenue.", BasePrice = 8.00m, IsRequired = false } },
            { 103, new Feature { Id = 103, Name = "Multi-Location Support", Description = "Manage multiple store locations from one dashboard.", BasePrice = 12.00m, IsRequired = false } }
        };

        private static readonly Dictionary<int, AddOn> CustomPackageAddOns = new()
        {
            { 201, new AddOn { Id = 201, Name = "Premium Support", Description = "24/7 priority support via chat and email.", Price = 5.00m } },
            { 202, new AddOn { Id = 202, Name = "Custom Branding", Description = "Add your own logo and color scheme to the POS.", Price = 7.00m, Dependencies = new List<int> { 101 } } }
        };

        private static readonly Dictionary<int, UsageBasedPricing> CustomPackageUsagePricing = new()
        {
            { 101, new UsageBasedPricing { FeatureId = 101, Name = "API Calls", Unit = "requests", MinValue = 1000, MaxValue = 100000, PricePerUnit = 0.01m } },
            { 102, new UsageBasedPricing { FeatureId = 102, Name = "User Licenses", Unit = "users", MinValue = 1, MaxValue = 50, PricePerUnit = 5.00m } }
        };
    }

    public class CustomPricingRequest
    {
        public List<int> SelectedFeatures { get; set; } = new();
        public List<int> SelectedAddOns { get; set; } = new();
        public Dictionary<int, int> UsageLimits { get; set; } = new();
    }
}
