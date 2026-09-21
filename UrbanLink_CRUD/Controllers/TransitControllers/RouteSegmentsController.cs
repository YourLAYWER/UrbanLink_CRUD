using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Entities.Transit;

namespace UrbanLink_CRUD.Controllers.TransitControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RouteSegmentsController : ControllerBase
    {
        private readonly ULDBContext _context;

        public RouteSegmentsController(ULDBContext context)
        {
            _context = context;
        }

        // GET: api/RouteSegments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RouteSegment>>> GetRouteSegments()
        {
            return await _context.RouteSegments
                .Include(rs => rs.RouteStops)
                .ThenInclude(rs => rs.Stop)
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/RouteSegments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RouteSegment>> GetRouteSegment(int id)
        {
            var segment = await _context.RouteSegments
                .Include(rs => rs.RouteStops)
                .ThenInclude(rs => rs.Stop)
                .FirstOrDefaultAsync(rs => rs.Id == id);

            if (segment == null)
            {
                return NotFound(new { message = $"Route Segment with ID {id} not found." });
            }

            return segment;
        }

        // POST: api/RouteSegments
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RouteSegment>> CreateRouteSegment([FromBody] RouteSegment segment)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.RouteSegments.Add(segment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRouteSegment), new { id = segment.Id }, segment);
        }

        // PUT: api/RouteSegments/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRouteSegment(int id, [FromBody] RouteSegment segment)
        {
            if (id != segment.Id)
            {
                return BadRequest(new { message = "ID mismatch." });
            }

            _context.Entry(segment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SegmentExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/RouteSegments/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRouteSegment(int id)
        {
            var segment = await _context.RouteSegments.FindAsync(id);
            if (segment == null) return NotFound();

            _context.RouteSegments.Remove(segment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SegmentExists(int id)
        {
            return _context.RouteSegments.Any(e => e.Id == id);
        }
    }
}
