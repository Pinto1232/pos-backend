using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models;
using PosBackend.DTOs;
using PosBackend.DTOs.Common;
using PosBackend.Extensions;
using System.Threading.Tasks;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly PosDbContext _context;

        public CustomersController(PosDbContext context)
        {
            _context = context;
        }

        // GET: api/Customers - Paginated list with projections
        [HttpGet]
        public async Task<ActionResult<PagedResult<CustomerListDto>>> GetCustomers([FromQuery] PaginationParams pagination)
        {
            var query = _context.Customers
                .IncludeBasicCustomerInfo()
                .AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(pagination.Search))
            {
                query = query.Where(c => c.FirstName.Contains(pagination.Search) || 
                                        c.LastName.Contains(pagination.Search) ||
                                        c.Email.Contains(pagination.Search) ||
                                        c.Phone.Contains(pagination.Search));
            }

            // Apply sorting
            var sortProperty = pagination.SortBy?.ToLower() switch
            {
                "firstname" => nameof(Customer.FirstName),
                "lastname" => nameof(Customer.LastName),
                "email" => nameof(Customer.Email),
                "createdat" => nameof(Customer.CreatedAt),
                "lastvisit" => nameof(Customer.LastVisit),
                _ => nameof(Customer.LastVisit)
            };

            query = query.ApplyOrdering(sortProperty, pagination.SortOrder);

            // Project to DTO and paginate
            var projectedQuery = query.Select(c => new CustomerListDto
            {
                CustomerId = c.CustomerId,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone,
                LoyaltyPoints = c.LoyaltyPoint != null ? c.LoyaltyPoint.PointsBalance : 0,
                CreatedAt = c.CreatedAt,
                LastVisit = c.LastVisit,
                TotalOrders = c.Sales != null ? c.Sales.Count : 0,
                TotalSpent = c.Sales != null ? c.Sales.Sum(s => s.TotalAmount) : 0
            });

            var result = await projectedQuery.ToPagedResultAsync(pagination);
            return Ok(result);
        }

        // GET: api/Customers/summary - Lightweight summary for dropdowns
        [HttpGet("summary")]
        public async Task<ActionResult<IEnumerable<CustomerSummaryDto>>> GetCustomersSummary()
        {
            var customers = await _context.Customers
                .AsNoTracking()
                .Select(c => new CustomerSummaryDto
                {
                    CustomerId = c.CustomerId,
                    FullName = (c.FirstName + " " + c.LastName).Trim(),
                    Email = c.Email,
                    Phone = c.Phone
                })
                .OrderBy(c => c.FullName)
                .ToListAsync();

            return Ok(customers);
        }

        // GET: api/Customers/5 - Detailed view with projections
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDetailDto>> GetCustomer(int id)
        {
            var customer = await _context.Customers
                .IncludeFullCustomerInfo()
                .Where(c => c.CustomerId == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer.ToDetailDto());
        }

        // POST: api/Customers
        [HttpPost]
        public async Task<ActionResult<CustomerDetailDto>> CreateCustomer(CustomerCreateDto customerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check for duplicate email
            if (await _context.Customers.AnyAsync(c => c.Email == customerDto.Email))
            {
                return BadRequest("A customer with this email already exists");
            }

            var customer = customerDto.ToEntity();
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Create loyalty points record
            var loyaltyPoint = new LoyaltyPoint
            {
                CustomerId = customer.CustomerId,
                PointsBalance = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.LoyaltyPoints.Add(loyaltyPoint);
            await _context.SaveChangesAsync();

            // Return detailed customer
            var createdCustomer = await _context.Customers
                .IncludeFullCustomerInfo()
                .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

            return CreatedAtAction(nameof(GetCustomer), 
                new { id = customer.CustomerId }, 
                createdCustomer!.ToDetailDto());
        }

        // PUT: api/Customers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, CustomerUpdateDto customerDto)
        {
            if (id != customerDto.CustomerId)
            {
                return BadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null)
            {
                return NotFound();
            }

            // Check for duplicate email (excluding current customer)
            if (await _context.Customers.AnyAsync(c => c.Email == customerDto.Email && c.CustomerId != id))
            {
                return BadRequest("A customer with this email already exists");
            }

            customerDto.UpdateEntity(customer);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            // Check if customer has any sales/orders
            var hasOrders = await _context.Sales.AnyAsync(s => s.CustomerId == id);
            if (hasOrders)
            {
                return BadRequest("Cannot delete customer with existing orders. Consider deactivating instead.");
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}
