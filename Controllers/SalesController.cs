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
    public class SalesController : ControllerBase
    {
        private readonly PosDbContext _context;

        public SalesController(PosDbContext context)
        {
            _context = context;
        }

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

        [HttpPost]
        public async Task<ActionResult<Sale>> CreateSale(SaleCreateRequest request)
        {
            var store = await _context.Stores.FindAsync(request.StoreId);
            if (store == null)
                return BadRequest($"Store with ID {request.StoreId} not found");

            var terminal = await _context.Terminals.FindAsync(request.TerminalId);
            if (terminal == null)
                return BadRequest($"Terminal with ID {request.TerminalId} not found");

            var customer = await _context.Customers.FindAsync(request.CustomerId);
            if (customer == null)
                return BadRequest($"Customer with ID {request.CustomerId} not found");

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return BadRequest($"User with ID {request.UserId} not found");

            var sale = new Sale
            {
                StoreId = request.StoreId,
                Store = store,
                TerminalId = request.TerminalId,
                Terminal = terminal,
                CustomerId = request.CustomerId,
                Customer = customer,
                UserId = request.UserId,
                User = user,
                SaleDate = DateTime.UtcNow,
                TotalAmount = 0,
                Invoice = new Invoice
                {
                    InvoiceNumber = $"INV-{DateTime.UtcNow.Ticks}",
                    IssuedDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(30),
                    IsPaid = false,
                    TotalAmount = 0,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            if (request.SaleItems != null && request.SaleItems.Any())
            {
                foreach (var item in request.SaleItems)
                {
                    var productVariant = await _context.ProductVariants.FindAsync(item.VariantId);
                    if (productVariant == null)
                        return BadRequest($"ProductVariant with ID {item.VariantId} not found");

                    var saleItem = new SaleItem
                    {
                        SaleId = sale.SaleId,
                        Sale = sale,
                        VariantId = item.VariantId,
                        ProductVariant = productVariant,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Discount = item.Discount
                    };

                    _context.SaleItems.Add(saleItem);
                }
            }

            if (request.Payments != null && request.Payments.Any())
            {
                foreach (var payment in request.Payments)
                {
                    var salePayment = new Payment
                    {
                        SaleId = sale.SaleId,
                        Sale = sale,
                        Amount = payment.Amount,
                        PaymentMethod = payment.PaymentMethod,
                        Timestamp = DateTime.UtcNow
                    };

                    _context.Payments.Add(salePayment);
                }
            }

            sale.TotalAmount = request.SaleItems?.Sum(item =>
                item.Quantity * item.UnitPrice - item.Discount) ?? 0;

            sale.Invoice.TotalAmount = sale.TotalAmount;

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSale), new { id = sale.SaleId }, sale);
        }

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

        [HttpGet("top-products")]
        public async Task<ActionResult<object>> GetTopSellingProducts()
        {
            var topProducts = await _context.SaleItems
                .Include(si => si.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .GroupBy(si => si.ProductVariant != null && si.ProductVariant.Product != null
                    ? si.ProductVariant.Product.Name
                    : "Unknown Product")
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

    public class SaleCreateRequest
    {
        public int StoreId { get; set; }
        public int TerminalId { get; set; }
        public int CustomerId { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public List<SaleItemCreateRequest> SaleItems { get; set; } = new List<SaleItemCreateRequest>();
        public List<PaymentCreateRequest> Payments { get; set; } = new List<PaymentCreateRequest>();
    }

    public class SaleItemCreateRequest
    {
        public int VariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
    }

    public class PaymentCreateRequest
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
