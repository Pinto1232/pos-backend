using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models; // Adjust to your namespace

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

         // GET: api/PricingPackages
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

            // Return a paginated response
            return Ok(new
            {
                TotalItems = totalItems,
                Data = packages
            });
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public ActionResult<string> PublicEndpoint()
        {
             return "This endpoint does not require a token!";
        }
    }
}
