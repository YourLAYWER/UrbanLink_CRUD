using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Dtos.Transit;
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
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RouteStop>> AddStopToSegment([FromBody] RouteStop routeStop)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var segmentExists = await _context.RouteSegments.AnyAsync(s => s.Id == routeStop.SegmentId);
            var stopExists = await _context.Stops.AnyAsync(s => s.Id == routeStop.StopId);

            if (!segmentExists || !stopExists)
            {
                return BadRequest(new { message = "Invalid SegmentId or StopId provided." });
            }

            var duplicateExists = await _context.RouteStops
                .AnyAsync(rs => rs.SegmentId == routeStop.SegmentId && rs.StopId == routeStop.StopId);

            if (duplicateExists)
            {
                return BadRequest(new { message = "This stop has already been added to this route segment." });
            }

            int currentStopCount = await _context.RouteStops
                .Where(rs => rs.SegmentId == routeStop.SegmentId)
                .CountAsync();

            routeStop.StopSequence = currentStopCount + 1;

            _context.RouteStops.Add(routeStop);
            await _context.SaveChangesAsync();

            return Ok(routeStop);
        }

        // PUT: api/RouteStops/segment/1/reorder
        // Body: { "orderedStopIds": [5, 12, 3] } — the new travel order for this segment.
        [HttpPut("segment/{segmentId:int}/reorder")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReorderStops(int segmentId, [FromBody] ReorderRouteStopsRequestDto request)
        {
            var stops = await _context.RouteStops
                .Where(rs => rs.SegmentId == segmentId)
                .ToListAsync();

            if (stops.Count == 0) return NotFound(new { message = $"No stops found for segment {segmentId}." });

            var currentIds = stops.Select(s => s.StopId).ToHashSet();
            var requestedIds = request.OrderedStopIds.ToHashSet();

            if (!currentIds.SetEquals(requestedIds))
            {
                return BadRequest(new { message = "orderedStopIds must contain exactly the stops currently on this segment." });
            }

            for (int i = 0; i < request.OrderedStopIds.Count; i++)
            {
                var stop = stops.First(s => s.StopId == request.OrderedStopIds[i]);
                stop.StopSequence = i + 1;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/RouteStops/1/5
        [HttpDelete("{segmentId}/{stopId}")]
        [Authorize(Roles = "Admin")]
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
