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
    public class InventoryController : ControllerBase
    {
        private readonly PosDbContext _context;

        public InventoryController(PosDbContext context)
        {
            _context = context;
        }

        // GET: api/Inventory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inventory>>> GetInventoryItems()
        {
            var inventories = await _context.Inventories
                .Include(inventoryItem => inventoryItem.Store)
                .Include(inventoryItem => inventoryItem.ProductVariant)
                .Include(inventoryItem => inventoryItem.StockAlerts)
                .AsNoTracking()
                .ToListAsync();

            return inventories ?? new List<Inventory>();
        }

        // GET: api/Inventory/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Inventory>> GetSpecificInventory(int id)
        {
            var inventoryItem = await _context.Inventories
                .Include(item => item.Store)
                .Include(item => item.ProductVariant)
                .Include(item => item.StockAlerts)
                .FirstOrDefaultAsync(item => item.InventoryId == id);

            if (inventoryItem == null)
            {
                return NotFound();
            }

            return inventoryItem;
        }

        // POST: api/Inventory
        [HttpPost]
        public async Task<ActionResult<Inventory>> CreateInventoryItem(Inventory inventoryItem)
        {
            inventoryItem.LastUpdated = DateTime.UtcNow;
            _context.Inventories.Add(inventoryItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSpecificInventory), new { id = inventoryItem.InventoryId }, inventoryItem);
        }

        // PUT: api/Inventory/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInventoryItem(int id, Inventory inventoryItem)
        {
            if (id != inventoryItem.InventoryId)
            {
                return BadRequest();
            }

            inventoryItem.LastUpdated = DateTime.UtcNow;
            _context.Entry(inventoryItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventoryItemExists(id))
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

        // DELETE: api/Inventory/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventoryItem(int id)
        {
            var inventoryItem = await _context.Inventories.FindAsync(id);
            if (inventoryItem == null)
            {
                return NotFound();
            }

            _context.Inventories.Remove(inventoryItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Additional method to update inventory quantity
        [HttpPatch("{id}/quantity")]
        public async Task<IActionResult> UpdateInventoryItemQuantity(int id, [FromBody] int newQuantity)
        {
            var inventoryItem = await _context.Inventories.FindAsync(id);
            if (inventoryItem == null)
            {
                return NotFound();
            }

            inventoryItem.Quantity = newQuantity;
            inventoryItem.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InventoryItemExists(int id)
        {
            return _context.Inventories.Any(e => e.InventoryId == id);
        }
    }
}
