using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Entities.Transit;
using UrbanLink_CRUD.Models.Enums;

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


        /// <summary>
        /// Retrieves a lightweight list of transit providers filtered by transport type (e.g., "Bus", "Rail").
        /// </summary>
        /// <param name="transportType">The type of transport (case-insensitive)</param>
        [HttpGet("type/{transportType}")]
        public async Task<IActionResult> GetProvidersByType(string transportType)
        {
            // 1. Safely parse the URL string into the strong TransitType Enum (true ignores case)
            if (!Enum.TryParse<TransitType>(transportType, true, out var parsedTransitType))
            {
                return BadRequest(new
                {
                    message = $"Invalid transport type: {transportType}. Valid options are Bus, Train, Tram, Ferry, Subway."
                });
            }

            // 2. Query the database directly comparing Enum to Enum!
            var providers = await _context.TransitProviders
                .Where(p => p.Type == parsedTransitType)
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .OrderBy(p => p.Name)
                .ToListAsync();

            if (providers.Count == 0)
            {
                // Using the parsed enum here ensures proper formatting (e.g., "Bus" instead of "bus")
                return NotFound(new { message = $"No providers found for transport type: {parsedTransitType}" });
            }

            return Ok(providers);
        }

        /// <summary>
        /// Retrieves a lightweight list of transit providers for UI dropdowns.
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetProviderSummaries()
        {
            var providers = await _context.TransitProviders
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name
                })
                // Order them alphabetically
                .OrderBy(p => p.Name)
                .ToListAsync();

            if (providers.Count == 0)
            {
                return NotFound(new { message = "No transit providers found." });
            }

            return Ok(providers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransitProvider>> GetTransitProvider(int id)
        {
            var provider = await _context.TransitProviders.FindAsync(id);
            if (provider == null) return NotFound(new { message = $"TransitProvider with ID {id} not found." });
            return provider;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TransitProvider>> CreateTransitProvider(TransitProvider provider)
        {
            _context.TransitProviders.Add(provider);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTransitProvider), new { id = provider.Id }, provider);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTransitProvider(int id, TransitProvider provider)
        {
           if (id != provider.Id) return BadRequest(new { message = "ID in URL does not match ID in request body." });
            _context.Entry(provider).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
               if (!_context.TransitProviders.Any(p => p.Id == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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