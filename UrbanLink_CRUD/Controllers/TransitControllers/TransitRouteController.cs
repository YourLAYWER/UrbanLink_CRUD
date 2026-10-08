using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Dtos.Transit;
using UrbanLink_CRUD.Models.Entities.Transit;

namespace UrbanLink_CRUD.Controllers.TransitControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransitRouteController : ControllerBase
    {
        private readonly ULDBContext _context;

        public TransitRouteController(ULDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all transit routes and their schedules for a specific provider.
        /// </summary>
        [HttpGet("provider/{providerId}")]
        public async Task<IActionResult> GetRoutesByProvider(int providerId)
        {
            var providerExists = await _context.TransitProviders.AnyAsync(p => p.Id == providerId);
            if (!providerExists)
            {
                return NotFound(new { message = $"Transit Provider with ID {providerId} was not found." });
            }

            // Only raw column values are selected here so EF Core can translate the whole
            // query to SQL. TimeSpan.ToString("hh\:mm") has no SQL equivalent, so the
            // "HH:mm" formatting is done in memory below.
            var routeData = await _context.TransitRoutes
                .Where(r => r.ProviderId == providerId)
                .Select(r => new
                {
                    r.Id,
                    r.RouteName,
                    r.TransportType,
                    r.BaseFare,
                    Schedules = r.Schedules.Select(s => new
                    {
                        s.ScheduleId,
                        s.DaysOfOperation,
                        s.FirstDepartureTime,
                        s.LastDepartureTime,
                        s.FrequencyMinutes
                    }).ToList()
                })
                .ToListAsync();

            var routes = routeData.Select(r => new
            {
                RouteId = r.Id,
                RouteName = r.RouteName,
                TransportType = r.TransportType,
                BaseFare = r.BaseFare,
                Schedules = r.Schedules.Select(s => new
                {
                    ScheduleId = s.ScheduleId,
                    DaysOfOperation = s.DaysOfOperation,
                    FirstDeparture = s.FirstDepartureTime.ToString(@"hh\:mm"),
                    LastDeparture = s.LastDepartureTime.ToString(@"hh\:mm"),
                    FrequencyMinutes = s.FrequencyMinutes
                }).ToList()
            }).ToList();

            if (routes.Count == 0)
            {
                return Ok(new { message = "This provider currently has no active routes.", data = routes });
            }

            return Ok(new { message = $"Successfully retrieved {routes.Count} routes.", data = routes });
        }

        [HttpGet("routes")]
        public async Task<IActionResult> GetAllRoutes()
        {
            var routes = await _context.TransitRoutes
                .Select(r => new { id = r.Id, name = r.RouteName })
                .ToListAsync();

            return Ok(new { data = routes });
        }

        // GET: api/TransitRoute/5
        // Full detail for the Route & Fare Studio: metadata, ordered segments/stops, schedules.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetRoute(int id)
        {
            var route = await _context.TransitRoutes
                .Include(r => r.Provider)
                .Include(r => r.Schedules)
                .Include(r => r.Segments)
                    .ThenInclude(s => s.RouteStops)
                        .ThenInclude(rs => rs.Stop)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (route == null) return NotFound(new { message = $"Route with ID {id} not found." });

            return Ok(new
            {
                id = route.Id,
                name = route.RouteName,
                transportType = route.TransportType,
                baseFare = route.BaseFare,
                providerId = route.ProviderId,
                providerName = route.Provider != null ? route.Provider.Name : null,
                schedules = route.Schedules.Select(s => new
                {
                    scheduleId = s.ScheduleId,
                    daysOfOperation = s.DaysOfOperation,
                    firstDeparture = s.FirstDepartureTime.ToString(@"hh\:mm"),
                    lastDeparture = s.LastDepartureTime.ToString(@"hh\:mm"),
                    frequencyMinutes = s.FrequencyMinutes
                }),
                segments = route.Segments.Select(seg => new
                {
                    id = seg.Id,
                    originStopId = seg.OriginStopId,
                    destinationStopId = seg.DestinationStopId,
                    travelTimeMinutes = seg.TravelTimeMinutes,
                    stops = seg.RouteStops
                        .OrderBy(rs => rs.StopSequence)
                        .Select(rs => new
                        {
                            stopId = rs.StopId,
                            stopName = rs.Stop != null ? rs.Stop.Name : null,
                            sequence = rs.StopSequence
                        })
                })
            });
        }

        // POST: api/TransitRoute
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRoute([FromBody] CreateRouteRequestDto request)
        {
            var providerExists = await _context.TransitProviders.AnyAsync(p => p.Id == request.ProviderId);
            if (!providerExists) return BadRequest(new { message = $"Provider {request.ProviderId} does not exist." });

            var route = new TransitRoute
            {
                RouteName = request.RouteName.Trim(),
                TransportType = request.TransportType,
                BaseFare = request.BaseFare,
                ProviderId = request.ProviderId
            };

            _context.TransitRoutes.Add(route);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoute), new { id = route.Id }, new { route.Id });
        }

        // PUT: api/TransitRoute/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRoute(int id, [FromBody] UpdateRouteRequestDto request)
        {
            var route = await _context.TransitRoutes.FindAsync(id);
            if (route == null) return NotFound(new { message = $"Route with ID {id} not found." });

            var providerExists = await _context.TransitProviders.AnyAsync(p => p.Id == request.ProviderId);
            if (!providerExists) return BadRequest(new { message = $"Provider {request.ProviderId} does not exist." });

            route.RouteName = request.RouteName.Trim();
            route.TransportType = request.TransportType;
            route.BaseFare = request.BaseFare;
            route.ProviderId = request.ProviderId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/TransitRoute/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            var route = await _context.TransitRoutes.FindAsync(id);
            if (route == null) return NotFound(new { message = $"Route with ID {id} not found." });

            var hasSegments = await _context.RouteSegments.AnyAsync(s => s.RouteId == id);
            if (hasSegments)
            {
                return BadRequest(new { message = "Delete this route's segments first, then delete the route." });
            }

            _context.TransitRoutes.Remove(route);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
