using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Dtos.TransitDtos;
using UrbanLink_CRUD.Models.Entities.Transit;
using UrbanLink_CRUD.Services.RoutingLogic;

namespace UrbanLink_CRUD.Controllers.TransitControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JourneyController : ControllerBase
    {
        private readonly ULDBContext _context;

        // 1. Updated to use the Interface rather than the nested class wrapper
        private readonly ITransitRoutingService _routingService;

        public JourneyController(ULDBContext context, ITransitRoutingService routingService)
        {
            _context = context;
            _routingService = routingService;
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateRoute([FromBody] RouteCalculateRequest request)
        {
            if (request.StartStopId == request.EndStopId)
                return BadRequest(new { message = "Start and destination cannot be the same." });

            // 1. Run your fantastic routing engine
            var routingResult = _routingService.ComputeFastestRoute(request.StartStopId, request.EndStopId);

            // -1 indicates no path was found
            if (routingResult.TotalTimeMinutes == -1 || !routingResult.Legs.Any())
                return NotFound(new { message = "No valid route found between these stops." });

            // 2. Tally up the total base cost from all the legs
            decimal totalBaseCost = routingResult.Legs.Sum(leg => leg.LegFare);

            var responseSegments = new List<RouteSegmentDto>();

            // Start the clock from right now (or a user-selected departure time)
            DateTime currentRouteTime = DateTime.Now;

            // 3. Translate the raw IDs into human-readable data for Android
            foreach (var leg in routingResult.Legs)
            {
                // Fetch the actual names from the database
                var startStop = await _context.Stops.FindAsync(leg.StartStopId);
                var routeInfo = await _context.TransitRoutes.FindAsync(leg.RouteId);

                responseSegments.Add(new RouteSegmentDto
                {
                    StopId = leg.StartStopId,
                    StopName = startStop?.Name ?? "Unknown Stop",
                    TransitLine = routeInfo?.RouteName ?? "Unknown Route",
                    ArrivalTime = currentRouteTime.ToString("HH:mm") // Formats as 08:40
                });

                // Add the travel time of this leg to our running clock
                currentRouteTime = currentRouteTime.AddMinutes(leg.LegDurationMinutes);
            }

            // 4. Add the final destination node to the end of the timeline
            var finalStop = await _context.Stops.FindAsync(request.EndStopId);
            responseSegments.Add(new RouteSegmentDto
            {
                StopId = request.EndStopId,
                StopName = finalStop?.Name ?? "Destination",
                TransitLine = "Arrive",
                ArrivalTime = currentRouteTime.ToString("HH:mm")
            });

            // 5. Package it all up for Android!
            var response = new RouteCalculateResponse
            {
                BaseCost = totalBaseCost,
                EstimatedDurationMinutes = routingResult.TotalTimeMinutes,
                Segments = responseSegments
            };

            return Ok(response);
        }

        [HttpPost("book")]
        [Authorize]
        public IActionResult BookJourney([FromBody] BookingRequestDto request)
        {
            // Extract Identity
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null) return Unauthorized();
            int userId = int.Parse(userIdString);

            // --- EXECUTING OUR ALGORITHM ---
            var routingResult = _routingService.ComputeFastestRoute(request.StartStopId, request.EndStopId);

            // Handle the "No Route Found" scenario gracefully
            if (routingResult.TotalTimeMinutes == -1 || routingResult.Legs.Count == 0)
            {
                return BadRequest(new { message = "No valid route found between these locations." });
            }

            // Calculate the total fare using the read-only algorithm records
            decimal baseTotalCost = routingResult.Legs.Sum(l => l.LegFare);

            // Apply Student/Pensioner Discounts based on JWT Role
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "Student")
            {
                baseTotalCost *= 0.85m;
            }

            // Build the Master Journey Entity for the database
            var journey = new Journey
            {
                UserId = userId,
                StartStopId = request.StartStopId,
                EndStopId = request.EndStopId,
                TotalCost = baseTotalCost,
                TotalDurationMinutes = routingResult.TotalTimeMinutes,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,

                // 2. Explicitly referenced the EF Core Entity namespace to prevent compiler confusion
                Legs = new List<UrbanLink_CRUD.Models.Entities.Transit.JourneyLeg>()
            };
            // Map your algorithm's output to the EF Core Database Entities
            int sequence = 1;
            foreach (var algLeg in routingResult.Legs)
            {
                // 3. Same here - map the algorithm record (algLeg) into the EF Core entity
                journey.Legs.Add(new UrbanLink_CRUD.Models.Entities.Transit.JourneyLeg
                {
                    SequenceOrder = sequence,
                    RouteId = algLeg.RouteId,
                    BoardingStopId = algLeg.StartStopId,
                    DestinationStopId = algLeg.EndStopId,
                    LegDurationMinutes = algLeg.LegDurationMinutes,
                    LegFare = algLeg.LegFare,
                    Status = "Active",
                    TicketReference = Guid.NewGuid()
                });
                sequence++;
            }

            // Save transaction
            _context.Journeys.Add(journey);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Journey booked successfully.",
                journeyId = journey.Id,
                ticketReference = journey.TicketReference,
                totalCost = journey.TotalCost,
                estimatedDurationMinutes = journey.TotalDurationMinutes
            });
        }

    }
}