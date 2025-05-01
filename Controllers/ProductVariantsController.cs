using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductVariantsController : ControllerBase
    {
        private readonly PosDbContext _context;

        public ProductVariantsController(PosDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductVariant>>> GetProductVariants()
        {
            return await _context.ProductVariants
                .Include(pv => pv.Product)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductVariant>> GetProductVariant(int id)
        {
            var productVariant = await _context.ProductVariants
                .Include(pv => pv.Product)
                .FirstOrDefaultAsync(pv => pv.VariantId == id);

            if (productVariant == null)
            {
                return NotFound();
            }

            return productVariant;
        }

        [HttpPost]
        public async Task<ActionResult<ProductVariant>> CreateProductVariant(ProductVariant productVariant)
        {
            // Verify that the referenced product exists
            var product = await _context.Products.FindAsync(productVariant.ProductId);
            if (product == null)
            {
                return BadRequest($"Product with ID {productVariant.ProductId} not found");
            }

            _context.ProductVariants.Add(productVariant);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductVariant), new { id = productVariant.VariantId }, productVariant);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProductVariant(int id, ProductVariant productVariant)
        {
            if (id != productVariant.VariantId)
            {
                return BadRequest();
            }

            _context.Entry(productVariant).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductVariantExists(id))
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductVariant(int id)
        {
            var productVariant = await _context.ProductVariants.FindAsync(id);
            if (productVariant == null)
            {
                return NotFound();
            }

            _context.ProductVariants.Remove(productVariant);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductVariantExists(int id)
        {
            return _context.ProductVariants.Any(e => e.VariantId == id);
        }
    }
}
