using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Entities.Transit;

namespace UrbanLink_CRUD.Controllers.TransitControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StopsController : ControllerBase
    {
        private readonly ULDBContext _context;

        public StopsController(ULDBContext context)
        {
            _context = context;
        }

        // GET: api/Stops
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Stop>>> GetStops()
        {
            return await _context.Stops
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/Stops/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Stop>> GetStop(int id)
        {
            var stop = await _context.Stops.FindAsync(id);

            if (stop == null)
            {
                return NotFound(new { message = $"Stop with ID {id} not found." });
            }

            return stop;
        }

        // POST: api/Stops
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Stop>> CreateStop([FromBody] Stop stop)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Stops.Add(stop);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStop), new { id = stop.Id }, stop);
        }

        // PUT: api/Stops/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateStop(int id, [FromBody] Stop stop)
        {
            if (id != stop.Id)
            {
                return BadRequest(new { message = "ID mismatch." });
            }

            _context.Entry(stop).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StopExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Stops/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteStop(int id)
        {
            var stop = await _context.Stops.FindAsync(id);
            if (stop == null) return NotFound();

            _context.Stops.Remove(stop);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StopExists(int id)
        {
            return _context.Stops.Any(e => e.Id == id);
        }
    }
}
