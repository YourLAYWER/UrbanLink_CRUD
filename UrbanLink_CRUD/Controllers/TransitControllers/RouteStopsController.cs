using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Entities.Transit;

namespace UrbanLink_CRUD.Controllers.TransitControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RouteStopsController : ControllerBase
    {
        private readonly ULDBContext _context;

        public RouteStopsController(ULDBContext context)
        {
            _context = context;
        }

        // GET: api/RouteStops
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RouteStop>>> GetRouteStops()
        {
            return await _context.RouteStops
                .Include(rs => rs.Stop)
                .Include(rs => rs.RouteSegment)
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/RouteStops/segment/1
        [HttpGet("segment/{segmentId}")]
        public async Task<ActionResult<IEnumerable<RouteStop>>> GetStopsForSegment(int segmentId)
        {
            return await _context.RouteStops
                .Where(rs => rs.SegmentId == segmentId)
                .OrderBy(rs => rs.StopSequence)
                .Include(rs => rs.Stop)
                .AsNoTracking()
                .ToListAsync();
        }

        // POST: api/RouteStops
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<RouteStop>> AddStopToSegment([FromBody] RouteStop routeStop)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var segmentExists = await _context.RouteSegments.AnyAsync(s => s.Id == routeStop.SegmentId);
            var stopExists = await _context.Stops.AnyAsync(s => s.Id == routeStop.StopId);

            if (!segmentExists || !stopExists)
            {
                return BadRequest(new { message = "Invalid SegmentId or StopId provided." });
            }

            _context.RouteStops.Add(routeStop);
            await _context.SaveChangesAsync();

            return Ok(routeStop);
        }

        // DELETE: api/RouteStops/1/5
        [HttpDelete("{segmentId}/{stopId}")]
        [Authorize]
        public async Task<IActionResult> RemoveStopFromSegment(int segmentId, int stopId)
        {
            var routeStop = await _context.RouteStops.FindAsync(segmentId, stopId);
            if (routeStop == null) return NotFound();

            _context.RouteStops.Remove(routeStop);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}