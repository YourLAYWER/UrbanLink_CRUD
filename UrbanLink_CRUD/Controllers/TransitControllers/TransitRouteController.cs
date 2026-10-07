using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;

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
        /// <param name="providerId">The ID of the Transit Provider (e.g., Algoa Bus)</param>
        [HttpGet("provider/{providerId}")]
        public async Task<IActionResult> GetRoutesByProvider(int providerId)
        {
            // 1. First check if the provider even exists to give a clear error message
            var providerExists = await _context.TransitProviders.AnyAsync(p => p.Id == providerId);
            if (!providerExists)
            {
                return NotFound(new { message = $"Transit Provider with ID {providerId} was not found." });
            }

            // 2. Fetch the routes and eagerly load their schedules
            var routes = await _context.TransitRoutes
                .Where(r => r.ProviderId == providerId)
                .Include(r => r.Schedules) // Pull in the new schedules we just created!
                .Select(r => new
                {
                    RouteId = r.Id,
                    RouteName = r.RouteName,
                    TransportType = r.TransportType,
                    BaseFare = r.BaseFare,

                    // Format the schedules cleanly for the Android client
                    Schedules = r.Schedules.Select(s => new
                    {
                        ScheduleId = s.ScheduleId,
                        DaysOfOperation = s.DaysOfOperation,

                        // Convert TimeSpan to a clean "HH:mm" string
                        FirstDeparture = s.FirstDepartureTime.ToString(@"hh\:mm"),
                        LastDeparture = s.LastDepartureTime.ToString(@"hh\:mm"),

                        FrequencyMinutes = s.FrequencyMinutes
                    }).ToList()
                })
                .ToListAsync();

            // 3. Handle the edge case where a provider exists but has no routes yet
            if (routes.Count == 0)
            {
                return Ok(new
                {
                    message = "This provider currently has no active routes.",
                    data = routes
                });
            }

            // 4. Return the structured data
            return Ok(new
            {
                message = $"Successfully retrieved {routes.Count} routes.",
                data = routes
            });
        }

        [HttpGet("all-map-routes")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllMapRoutes()
        {
            var routes = await _context.TransitRoutes
                .Select(r => new
                {
                    routeId = r.Id,
                    routeName = r.RouteName,
                    transportType = r.TransportType.ToString(), // "Bus" or "Train"
                    encodedPolyline = r.RouteMapPath
                })
                .Where(r => !string.IsNullOrEmpty(r.encodedPolyline))
                .ToListAsync();

            if (!routes.Any())
                return NotFound(new { message = "No map routes available." });

            return Ok(routes);
        }

        [HttpGet("routes")]
        public async Task<IActionResult> GetAllRoutes()
        {
            // Simply returning ID and Name to keep the JSON payload lightweight
            var routes = await _context.TransitRoutes
                .Select(r => new
                {
                    id = r.Id,
                    name = r.RouteName
                })
                .ToListAsync();

            return Ok(new { data = routes });
        }
    }
}
