using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Store)
                .Include(o => o.OrderItems)
                .ToListAsync();
        }

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

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] OrderCreateRequest request)
        {
            var customer = await _context.Customers.FindAsync(request.CustomerId);
            if (customer == null)
            {
                return BadRequest($"Customer with ID {request.CustomerId} not found");
            }

            var store = await _context.Stores.FindAsync(request.StoreId);
            if (store == null)
            {
                return BadRequest($"Store with ID {request.StoreId} not found");
            }

            var order = new Order
            {
                CustomerId = request.CustomerId,
                StoreId = request.StoreId,
                OrderDate = request.OrderDate.ToUniversalTime(),
                Status = string.IsNullOrEmpty(request.Status) ? "Pending" : request.Status,
                TotalAmount = request.TotalAmount,
                ShippingAddress = request.ShippingAddress,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            if (request.OrderItems != null && request.OrderItems.Any())
            {
                foreach (var item in request.OrderItems)
                {
                    var productVariant = await _context.ProductVariants.FindAsync(item.ProductVariantId);
                    if (productVariant == null)
                    {
                        return BadRequest($"ProductVariant with ID {item.ProductVariantId} not found");
                    }

                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        Order = order,
                        VariantId = item.ProductVariantId,
                        ProductVariant = productVariant,
                        Quantity = item.Quantity,
                        Price = item.UnitPrice
                    };

                    order.OrderItems.Add(orderItem);
                }

                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
        }

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

    public class OrderCreateRequest
    {
        public int CustomerId { get; set; }
        public int StoreId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string? Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Notes { get; set; }
        public List<OrderItemCreateRequest>? OrderItems { get; set; }
    }

    public class OrderItemCreateRequest
    {
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
