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

        // GET: api/PricingPackages/custom/features
        [HttpGet("custom/features")]
        public async Task<ActionResult<object>> GetCustomFeatures()
        {
            var features = await _context.CoreFeatures.ToListAsync();
            var addOns = await _context.AddOns.ToListAsync();
            var usageBasedPricing = await _context.UsageBasedPricing.ToListAsync();

            return Ok(new
            {
                CoreFeatures = features,
                AddOns = addOns,
                UsageBasedPricing = usageBasedPricing
            });
        }

        // POST: api/PricingPackages/custom/select - Updates selected features and add-ons
        [HttpPost("custom/select")]
        public async Task<IActionResult> SelectCustomPackage([FromBody] CustomSelectionRequest request)
        {
            var package = await _context.PricingPackages
                .Include(p => p.SelectedFeatures)
                .Include(p => p.SelectedAddOns)
                .Include(p => p.SelectedUsageBasedPricing)
                .FirstOrDefaultAsync(p => p.Id == request.PricingPackageId);

            if (package == null || package.Type.ToLower() != "custom")
                return NotFound("Custom package not found.");

            // Clear previous selections
            package.SelectedFeatures.Clear();
            package.SelectedAddOns.Clear();
            package.SelectedUsageBasedPricing.Clear();

            // Add new selections
            package.SelectedFeatures = request.SelectedFeatures
                .Select(f => new CustomPackageSelectedFeature { PricingPackageId = package.Id, FeatureId = f })
                .ToList();

            package.SelectedAddOns = request.SelectedAddOns
                .Select(a => new CustomPackageSelectedAddOn { PricingPackageId = package.Id, AddOnId = a })
                .ToList();

            package.SelectedUsageBasedPricing = request.UsageLimits
                .Select(u => new CustomPackageUsageBasedPricing { PricingPackageId = package.Id, UsageBasedPricingId = u.Key, Quantity = u.Value })
                .ToList();

            await _context.SaveChangesAsync();
            return Ok("Custom package updated successfully.");
        }

        // POST: api/PricingPackages/custom/calculate-price
        [HttpPost("custom/calculate-price")]
        public async Task<ActionResult<object>> CalculateCustomPrice([FromBody] CustomPricingRequest request)
        {
            decimal basePrice = 99.99m;
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
                int quantity = request.UsageLimits[usage.Id];
                totalPrice += quantity * usage.PricePerUnit;
            }

            return Ok(new
            {
                BasePrice = basePrice,
                TotalPrice = totalPrice
            });
        }
    }

    public class CustomSelectionRequest
    {
        public int PricingPackageId { get; set; }
        public List<int> SelectedFeatures { get; set; } = new();
        public List<int> SelectedAddOns { get; set; } = new();
        public Dictionary<int, int> UsageLimits { get; set; } = new();
    }

    public class CustomPricingRequest
    {
        public List<int> SelectedFeatures { get; set; } = new();
        public List<int> SelectedAddOns { get; set; } = new();
        public Dictionary<int, int> UsageLimits { get; set; } = new();
    }
}
