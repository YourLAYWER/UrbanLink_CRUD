using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Timers;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Domain.Fare;
using UrbanLink_CRUD.Models.Domain.Journey;
using UrbanLink_CRUD.Models.Dtos.TransitDtos;
using UrbanLink_CRUD.Models.Entities.Transit;
using UrbanLink_CRUD.Models.Enums;
using UrbanLink_CRUD.Services.RoutingLogic;
using static UrbanLink_CRUD.Services.RoutingLogic.RoutingContex;

namespace UrbanLink_CRUD.Controllers.TransitControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JourneyController : ControllerBase
    {
        private readonly ULDBContext _context;
        private readonly ITransitRoutingService _routingService;
        private readonly IFareStrategyFactory _fareFactory;
        private readonly IRoutingStrategyFactory _routingFactory;
        private readonly IJourneyFactory _journeyFactory;

        public JourneyController(ULDBContext context, ITransitRoutingService routingService, IFareStrategyFactory fareFactory, IRoutingStrategyFactory routingFactory, IJourneyFactory journeyFactory)
        {
            _context = context;
            _routingService = routingService;
            _fareFactory = fareFactory;
            _routingFactory = routingFactory;
            _journeyFactory = journeyFactory;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Journey>>> GetTickets()
        {
            return await _context.Journeys.ToListAsync();
        }

        // Get All user journey
        [HttpGet("history")]
        [Authorize]
        public async Task<IActionResult> GetJourneyHistory()
        {
            // 1. Extract the secure User ID from the JWT Token
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            int userId = int.Parse(userIdString);

            // 2. Query the normalized UserJourneys table using LINQ syntax
            var history = await (from uj in _context.UserJourneys
                                 join startStop in _context.Stops on uj.StartStopID equals startStop.Id
                                 join endStop in _context.Stops on uj.EndStopID equals endStop.Id
                                 where uj.UserID == userId
                                 orderby uj.CreatedAt descending
                                 select new
                                 {
                                     ticketReference = uj.TicketReference,

                                     // Format the date exactly how your Android XML expects it
                                     date = uj.CreatedAt.ToString("dd MMM yyyy • HH:mm"),

                                     // Raw number, appended on the Android side
                                     cost = uj.TotalCost,

                                     // Dynamically build the "A -> B" string using the joined tables
                                     routeSummary = $"{startStop.Name} → {endStop.Name}",

                                     // Convert the strong Enum back to a string and force uppercase
                                     status = uj.Status.ToString().ToUpper()
                                 }).ToListAsync();

            // 3. Return the array to the app (returns [] if empty)
            return Ok(history);
        }


        // Calculate route before booking
        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateRoute([FromBody] RouteCalculateRequest request)
        {
            if (request.StartStopId == request.EndStopId)
                return BadRequest(new { message = "Start and destination cannot be the same." });

            // Use live server time instead of the hardcoded test date
            var currentDateTime = DateTime.UtcNow;

            // 1. STRATEGY EXECUTION: Route based on user preference (Fastest, Cheapest, etc.)
            var routingResult = _routingService.ComputeRoute(
                startStopId: request.StartStopId,
                targetStopId: request.EndStopId,
                userDepartureTime: currentDateTime,
                preference: "fastest" //
            );

            if (routingResult.TotalTimeMinutes == -1 || !routingResult.Legs.Any())
                return NotFound(new { message = "No valid route found between these stops." });

            // 2. Get the raw base cost from the selected route
            decimal totalBaseCost = routingResult.Legs.Sum(leg => leg.LegFare);

            // ==========================================
            // STRATEGY EXECUTION: Dynamic Fare Pricing
            // ==========================================
            string userProfile = User.FindFirstValue(ClaimTypes.Role) ?? "Standard";

            var fareContext = new FareCalculationContext(totalBaseCost, currentDateTime, userProfile);
            var fareStrategy = _fareFactory.GetStrategy(fareContext);
            decimal finalFare = fareStrategy.CalculateFare(fareContext);

            // ==========================================
            // BACKWARD MATH LOGIC (Unchanged)
            // ==========================================
            var finalArrivalTime = currentDateTime.AddMinutes(routingResult.TotalTimeMinutes);
            int physicalTravelTime = routingResult.Legs.Sum(l => l.LegDurationMinutes);
            int totalTransfers = routingResult.Legs.Count - 1;
            int transferPenalties = totalTransfers > 0 ? (totalTransfers * 10) : 0;
            var currentLegBoardingTime = finalArrivalTime.AddMinutes(-(physicalTravelTime + transferPenalties));

            var responseSegments = new List<RouteSegmentDto>();

            foreach (var leg in routingResult.Legs)
            {
                var startStop = await _context.Stops.FindAsync(leg.StartStopId);
                var routeInfo = await _context.TransitRoutes.FindAsync(leg.RouteId);
                var legArrivalTime = currentLegBoardingTime.AddMinutes(leg.LegDurationMinutes);

                responseSegments.Add(new RouteSegmentDto
                {
                    StopId = leg.StartStopId,
                    StopName = startStop?.Name ?? "Unknown Stop",
                    TransitLine = routeInfo?.RouteName ?? "Unknown Route",
                    ArrivalTime = currentLegBoardingTime.ToString("HH:mm")
                });

                currentLegBoardingTime = legArrivalTime.AddMinutes(10);
            }

            var finalStop = await _context.Stops.FindAsync(request.EndStopId);
            responseSegments.Add(new RouteSegmentDto
            {
                StopId = request.EndStopId,
                StopName = finalStop?.Name ?? "Destination",
                TransitLine = "Arrive",
                ArrivalTime = finalArrivalTime.ToString("HH:mm")
            });

            var response = new RouteCalculateResponse
            {
                BaseCost = totalBaseCost,
                FinalFare = finalFare,
                EstimatedDurationMinutes = routingResult.TotalTimeMinutes,
                Segments = responseSegments
            };

            return Ok(response);
        }

        [HttpPost("book")]
        [Authorize]
        public async Task<IActionResult> BookJourney([FromBody] BookingRequestDto request)
        {
            // 1. Resolve the algorithm based on user preference
            var routingStrategy = _routingFactory.GetStrategy("fastest");// Might add preference later be dynamic based on user preference or other factors

            // 2. Execute the chosen algorithm
            var routeContext = new RoutingContext(request.StartStopId, request.EndStopId, DateTime.UtcNow);
            var routingResult = _routingService.ComputeRoute(
                startStopId: request.StartStopId,
                targetStopId: request.EndStopId,
                userDepartureTime: DateTime.UtcNow,
                preference: "fastest" // Pass the user's choice here
            );


            if (routingResult.TotalTimeMinutes == -1)
                return BadRequest(new { message = "Invalid route." });

            decimal trueBaseCost = routingResult.Legs.Sum(leg => leg.LegFare);

            // 2. Extract identity and role securely from the JWT token
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userProfile = User.FindFirstValue(ClaimTypes.Role) ?? "Standard";

            // 3. Re-run the Strategy Pattern to generate the authoritative final price
            var fareContext = new FareCalculationContext(trueBaseCost, DateTime.UtcNow, userProfile);
            decimal secureFinalFare = _fareFactory.GetStrategy(fareContext).CalculateFare(fareContext);

            // 4. Create the normalized UserJourney entity
            var userJourney = _journeyFactory.CreateUserJourney(
                int.Parse(userIdString),
                request.StartStopId,
                request.EndStopId,
                secureFinalFare,
                DateTime.UtcNow.AddMinutes(routingResult.TotalTimeMinutes)
            );

            _context.UserJourneys.Add(userJourney);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Journey purchased successfully",
                journeyId = userJourney.JourneyID,
                ticketReference = userJourney.TicketReference,
                totalCost = secureFinalFare,
                estimatedDurationMinutes = routingResult.TotalTimeMinutes,
                estimatedArrivalTime = userJourney.EstimatedArrivalTime.ToString("HH:MM"),
                TicketStatus = userJourney.Status.ToString()
            });
        }

        [HttpPatch("update-status")]
        [Authorize]
        public async Task<IActionResult> UpdateTicketStatus([FromBody] UpdateTicketStatusDto request)
        {
            // 1. Safely parse the string from Android into your strong C# Enum.
            // The 'true' parameter makes it case-insensitive, so "COMPLETED", "completed", or "Completed" all work.
            if (!Enum.TryParse<JourneyStatus>(request.Status, true, out var newStatus))
            {
                return BadRequest(new { message = "Invalid status. Must be Active, Completed, Cancelled, or Expired." });
            }

            // 2. Find the Journey in the database using the unique GUID
            var journey = await _context.Journeys
                .FirstOrDefaultAsync(j => j.TicketReference == request.TicketReference);

            if (journey == null)
            {
                return NotFound(new { message = "Ticket not found." });
            }

            // 3. Security Check: Prevent double-scanning using the Enum!
            if (journey.Status == JourneyStatus.Completed && newStatus == JourneyStatus.Completed)
            {
                return BadRequest(new { message = "Ticket has already been completed! Do not let passenger board." });
            }

            if (journey.Status == JourneyStatus.Expired)
            {
                return BadRequest(new { message = "Ticket is expired and no longer valid." });
            }

            // 4. Update the status
            journey.Status = newStatus;

            // Optional: Automatically stamp the RedeemedAt time when it gets completed
            if (newStatus == JourneyStatus.Completed && journey.RedeemedAt == null)
            {
                journey.RedeemedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // 5. Return success, converting the Enum back to a string for the Android JSON response
            return Ok(new
            {
                message = $"Ticket successfully marked as {newStatus}.",
                ticketReference = journey.TicketReference,
                newStatus = journey.Status.ToString()
            });
        }
    }
}