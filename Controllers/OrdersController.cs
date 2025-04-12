using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models;
using System.Threading.Tasks;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly PosDbContext _context;

        public OrdersController(PosDbContext context)
        {
            _context = context;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Store)
                .Include(o => o.OrderItems)
                .ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Store)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            // Set timestamps
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            // Set default status if not provided
            if (string.IsNullOrEmpty(order.Status))
            {
                order.Status = "Pending";
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
        }

        // PUT: api/Orders/5/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string newStatus)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Orders/status-summary
        [HttpGet("status-summary")]
        public async Task<ActionResult<object>> GetOrderStatusSummary()
        {
            var statusSummary = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                    TotalItems = g.Sum(o => o.OrderItems.Sum(oi => oi.Quantity))
                })
                .ToListAsync();

            return Ok(statusSummary);
        }
    }
}
