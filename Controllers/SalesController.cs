using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models;
using System.Threading.Tasks;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly PosDbContext _context;

        public SalesController(PosDbContext context)
        {
            _context = context;
        }

        // GET: api/Sales
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sale>>> GetSales()
        {
            return await _context.Sales
                .Include(s => s.Store)
                .Include(s => s.Terminal)
                .Include(s => s.Customer)
                .Include(s => s.User)
                .Include(s => s.SaleItems)
                .Include(s => s.Payments)
                .ToListAsync();
        }

        // GET: api/Sales/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Sale>> GetSale(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Store)
                .Include(s => s.Terminal)
                .Include(s => s.Customer)
                .Include(s => s.User)
                .Include(s => s.SaleItems)
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.SaleId == id);

            if (sale == null)
            {
                return NotFound();
            }

            return sale;
        }

        // POST: api/Sales
        [HttpPost]
        public async Task<ActionResult<Sale>> CreateSale(Sale sale)
        {
            // Set sale date to current time
            sale.SaleDate = DateTime.UtcNow;

            // Calculate total amount from sale items
            sale.TotalAmount = sale.SaleItems?.Sum(item =>
                item.Quantity * item.UnitPrice - item.Discount) ?? 0;

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSale), new { id = sale.SaleId }, sale);
        }

        // GET: api/Sales/Daily
        [HttpGet("daily")]
        public async Task<ActionResult<object>> GetDailySales()
        {
            var dailySales = await _context.Sales
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalSales = g.Sum(s => s.TotalAmount),
                    SalesCount = g.Count()
                })
                .OrderByDescending(x => x.Date)
                .Take(30)
                .ToListAsync();

            return Ok(dailySales);
        }

        // GET: api/Sales/TopProducts
        [HttpGet("top-products")]
        public async Task<ActionResult<object>> GetTopSellingProducts()
        {
            var topProducts = await _context.SaleItems
                .GroupBy(si => si.ProductVariant.Product.Name)
                .Select(g => new
                {
                    ProductName = g.Key,
                    TotalQuantity = g.Sum(si => si.Quantity),
                    TotalRevenue = g.Sum(si => si.Quantity * si.UnitPrice)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(10)
                .ToListAsync();

            return Ok(topProducts);
        }
    }
}
