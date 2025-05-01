using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models;
using System.Threading.Tasks;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TerminalsController : ControllerBase
    {
        private readonly PosDbContext _context;

        public TerminalsController(PosDbContext context)
        {
            _context = context;
        }

        // GET: api/Terminals
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Terminal>>> GetTerminals()
        {
            return await _context.Terminals
                .Include(t => t.Store)
                .ToListAsync();
        }

        // GET: api/Terminals/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Terminal>> GetTerminal(int id)
        {
            var terminal = await _context.Terminals
                .Include(t => t.Store)
                .FirstOrDefaultAsync(t => t.TerminalId == id);

            if (terminal == null)
            {
                return NotFound();
            }

            return terminal;
        }

        // POST: api/Terminals
        [HttpPost]
        public async Task<ActionResult<Terminal>> CreateTerminal(TerminalCreateRequest request)
        {
            // Validate store exists
            var store = await _context.Stores.FindAsync(request.StoreId);
            if (store == null)
            {
                return BadRequest($"Store with ID {request.StoreId} not found");
            }

            var terminal = new Terminal
            {
                StoreId = request.StoreId,
                Store = store,
                Name = request.Name,
                Status = request.Status
            };

            _context.Terminals.Add(terminal);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTerminal), new { id = terminal.TerminalId }, terminal);
        }

        // PUT: api/Terminals/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTerminal(int id, TerminalUpdateRequest request)
        {
            var terminal = await _context.Terminals.FindAsync(id);
            if (terminal == null)
            {
                return NotFound();
            }

            // Update properties
            terminal.Name = request.Name;
            terminal.Status = request.Status;

            // Only update store if it's changed
            if (terminal.StoreId != request.StoreId)
            {
                var store = await _context.Stores.FindAsync(request.StoreId);
                if (store == null)
                {
                    return BadRequest($"Store with ID {request.StoreId} not found");
                }

                terminal.StoreId = request.StoreId;
                terminal.Store = store;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TerminalExists(id))
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

        // DELETE: api/Terminals/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTerminal(int id)
        {
            var terminal = await _context.Terminals.FindAsync(id);
            if (terminal == null)
            {
                return NotFound();
            }

            _context.Terminals.Remove(terminal);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TerminalExists(int id)
        {
            return _context.Terminals.Any(e => e.TerminalId == id);
        }
    }

    public class TerminalCreateRequest
    {
        public int StoreId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
    }

    public class TerminalUpdateRequest
    {
        public int StoreId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
