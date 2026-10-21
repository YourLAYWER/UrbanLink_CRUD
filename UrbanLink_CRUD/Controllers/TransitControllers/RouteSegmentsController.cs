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
        public async Task<IActionResult> GetRouteSegments()
        {
            var segments = await _context.RouteSegments
                .AsNoTracking()
                .Select(rs => new
                {
                    rs.Id,
                    rs.RouteId,
                    routeName = rs.Route != null ? rs.Route.RouteName : null,
                    rs.OriginStopId,
                    originStopName = rs.OriginStop != null ? rs.OriginStop.Name : null,
                    rs.DestinationStopId,
                    destinationStopName = rs.DestinationStop != null ? rs.DestinationStop.Name : null,
                    rs.TravelTimeMinutes,

                    // Kept so any screen that already reads routeStops keeps working
                    routeStops = rs.RouteStops
                        .OrderBy(x => x.StopSequence)
                        .Select(x => new
                        {
                            x.SegmentId,
                            x.StopId,
                            x.StopSequence,
                            stop = x.Stop == null ? null : new { x.Stop.Id, x.Stop.Name }
                        })
                })
                .ToListAsync();

            return Ok(segments);
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

            if (segment.OriginStopId == segment.DestinationStopId)
                return BadRequest(new { message = "Origin and destination must be different." });

            if (segment.TravelTimeMinutes <= 0)
                return BadRequest(new { message = "Travel time must be more than 0 minutes." });

            _context.RouteSegments.Add(segment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRouteSegment), new { id = segment.Id }, segment);
        }

        // PUT: api/RouteSegments/5
        // Copies only the editable fields onto the saved segment, so nothing
        // else on the row can be overwritten with empty values.
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRouteSegment(int id, [FromBody] RouteSegment segment)
        {
            if (id != segment.Id)
            {
                return BadRequest(new { message = "ID mismatch." });
            }

            if (segment.OriginStopId == segment.DestinationStopId)
                return BadRequest(new { message = "Origin and destination must be different." });

            if (segment.TravelTimeMinutes <= 0)
                return BadRequest(new { message = "Travel time must be more than 0 minutes." });

            var existing = await _context.RouteSegments.FindAsync(id);
            if (existing == null) return NotFound();

            existing.OriginStopId = segment.OriginStopId;
            existing.DestinationStopId = segment.DestinationStopId;
            existing.RouteId = segment.RouteId;
            existing.TravelTimeMinutes = segment.TravelTimeMinutes;

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
        // Removes the segment's stops first, so the delete does not fail
        // on the foreign key from RouteStops to RouteSegments.
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRouteSegment(int id)
        {
            var segment = await _context.RouteSegments.FindAsync(id);
            if (segment == null) return NotFound();

            // Delete the stops by the SegmentId column itself, then the segment,
            // in one transaction so nothing is left half-deleted.
            await using var transaction = await _context.Database.BeginTransactionAsync();

            await _context.RouteStops
                .Where(rs => rs.SegmentId == id)
                .ExecuteDeleteAsync();

            _context.RouteSegments.Remove(segment);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return NoContent();
        }

        private bool SegmentExists(int id)
        {
            return _context.RouteSegments.Any(e => e.Id == id);
        }
    }
}