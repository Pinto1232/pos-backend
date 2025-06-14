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
    public class ProductsController : ControllerBase
    {
        private readonly PosDbContext _context;

        public ProductsController(PosDbContext context)
        {
            _context = context;
        }

        // GET: api/Products - Paginated list with projections
        [HttpGet]
        public async Task<ActionResult<PagedResult<ProductListDto>>> GetProducts([FromQuery] PaginationParams pagination)
        {
            var query = _context.Products
                .IncludeBasicProductInfo()
                .Where(p => !p.IsDeleted)
                .AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(pagination.Search))
            {
                query = query.Where(p => p.Name.Contains(pagination.Search) || 
                                        p.Description.Contains(pagination.Search) ||
                                        p.Category!.Name.Contains(pagination.Search) ||
                                        p.Supplier!.Name.Contains(pagination.Search));
            }

            // Apply sorting
            var sortProperty = pagination.SortBy?.ToLower() switch
            {
                "name" => nameof(Product.Name),
                "price" => nameof(Product.BasePrice),
                "category" => nameof(Product.Category.Name),
                "createdat" => nameof(Product.CreatedAt),
                _ => nameof(Product.CreatedAt)
            };

            query = query.ApplyOrdering(sortProperty, pagination.SortOrder);

            // Project to DTO and paginate
            var projectedQuery = query.Select(p => new ProductListDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                BasePrice = p.BasePrice,
                CategoryName = p.Category!.Name,
                SupplierName = p.Supplier!.Name,
                VariantCount = p.ProductVariants!.Count,
                HasVariants = p.ProductVariants!.Any(),
                CreatedAt = p.CreatedAt,
                LastUpdatedAt = p.LastUpdatedAt
            });

            var result = await projectedQuery.ToPagedResultAsync(pagination);
            return Ok(result);
        }

        // GET: api/Products/summary - Lightweight summary for dropdowns
        [HttpGet("summary")]
        public async Task<ActionResult<IEnumerable<ProductSummaryDto>>> GetProductsSummary()
        {
            var products = await _context.Products
                .Where(p => !p.IsDeleted)
                .AsNoTracking()
                .Select(p => new ProductSummaryDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    BasePrice = p.BasePrice
                })
                .OrderBy(p => p.Name)
                .ToListAsync();

            return Ok(products);
        }

        // GET: api/Products/5 - Detailed view with projections
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDetailDto>> GetProduct(int id)
        {
            var product = await _context.Products
                .IncludeFullProductInfo()
                .Where(p => p.ProductId == id && !p.IsDeleted)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product.ToDetailDto());
        }

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<ProductDetailDto>> CreateProduct(ProductCreateDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate foreign keys
            if (!await _context.Categories.AnyAsync(c => c.CategoryId == productDto.CategoryId))
            {
                return BadRequest("Invalid CategoryId");
            }

            if (!await _context.Suppliers.AnyAsync(s => s.SupplierId == productDto.SupplierId))
            {
                return BadRequest("Invalid SupplierId");
            }

            var product = productDto.ToEntity();
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Return detailed product
            var createdProduct = await _context.Products
                .IncludeFullProductInfo()
                .FirstOrDefaultAsync(p => p.ProductId == product.ProductId);

            return CreatedAtAction(nameof(GetProduct), 
                new { id = product.ProductId }, 
                createdProduct!.ToDetailDto());
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, ProductUpdateDto productDto)
        {
            if (id != productDto.ProductId)
            {
                return BadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && !p.IsDeleted);

            if (product == null)
            {
                return NotFound();
            }

            // Validate foreign keys
            if (!await _context.Categories.AnyAsync(c => c.CategoryId == productDto.CategoryId))
            {
                return BadRequest("Invalid CategoryId");
            }

            if (!await _context.Suppliers.AnyAsync(s => s.SupplierId == productDto.SupplierId))
            {
                return BadRequest("Invalid SupplierId");
            }

            productDto.UpdateEntity(product);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
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

        // DELETE: api/Products/5 - Soft delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && !p.IsDeleted);
            
            if (product == null)
            {
                return NotFound();
            }

            // Soft delete
            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;
            product.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id && !e.IsDeleted);
        }
    }
}
