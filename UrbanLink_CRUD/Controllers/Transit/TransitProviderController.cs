using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Entities.Transit;

namespace UrbanLink_CRUD.Controllers.Transit
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransitProviderController : ControllerBase
    {
        private readonly ULDBContext _context;

        public TransitProviderController(ULDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransitProvider>>> GetTransitProviders()
        {
            return await _context.TransitProviders.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransitProvider>> GetTransitProvider(int id)
        {
            var provider = await _context.TransitProviders.FindAsync(id);
            if (provider == null) return NotFound(new { message = $"TransitProvider with ID {id} not found." });
            return provider;
        }

        [HttpPost]
        public async Task<ActionResult<TransitProvider>> CreateTransitProvider(TransitProvider provider)
        {
            _context.TransitProviders.Add(provider);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTransitProvider), new { id = provider.ID }, provider);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransitProvider(int id, TransitProvider provider)
        {
            if (id != provider.ID) return BadRequest(new { message = "ID in URL does not match ID in request body." });
            _context.Entry(provider).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.TransitProviders.Any(p => p.ID == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransitProvider(int id)
        {
            var provider = await _context.TransitProviders.FindAsync(id);
            if (provider == null) return NotFound(new { message = $"TransitProvider with ID {id} not found." });
            _context.TransitProviders.Remove(provider);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}