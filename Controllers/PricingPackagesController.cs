using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models; // Adjust to your namespace
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PosBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // This endpoint requires a valid Keycloak token
    public class PricingPackagesController : ControllerBase
    {
        private readonly PosDbContext _context;

        public PricingPackagesController(PosDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PricingPackage>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (_context.PricingPackages == null)
            {
                return NotFound("Pricing packages not found.");
            }

            var packages = await _context.PricingPackages
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(packages);
        }

        // Example: A test unprotected endpoint
        [HttpGet("public")]
        [AllowAnonymous]
        public ActionResult<string> PublicEndpoint()
        {
            return "Anyone can see this.";
        }
    }
}
