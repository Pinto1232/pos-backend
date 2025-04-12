using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models;
using System.Threading.Tasks;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountsController : ControllerBase
    {
        private readonly PosDbContext _context;

        public DiscountsController(PosDbContext context)
        {
            _context = context;
        }

        // GET: api/Discounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Discount>>> GetDiscounts()
        {
            return await _context.Discounts
                .Include(d => d.Coupons)
                .Where(d => d.ActiveUntil == null || d.ActiveUntil > DateTime.UtcNow)
                .ToListAsync();
        }

        // GET: api/Discounts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Discount>> GetDiscount(int id)
        {
            var discount = await _context.Discounts
                .Include(d => d.Coupons)
                .FirstOrDefaultAsync(d => d.DiscountId == id);

            if (discount == null)
            {
                return NotFound();
            }

            return discount;
        }

        // POST: api/Discounts
        [HttpPost]
        public async Task<ActionResult<Discount>> CreateDiscount(Discount discount)
        {
            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDiscount), new { id = discount.DiscountId }, discount);
        }

        // GET: api/Discounts/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Discount>>> GetActiveDiscounts()
        {
            return await _context.Discounts
                .Where(d =>
                    (d.ActiveUntil == null || d.ActiveUntil > DateTime.UtcNow) &&
                    d.Value > 0)
                .ToListAsync();
        }
    }
}
