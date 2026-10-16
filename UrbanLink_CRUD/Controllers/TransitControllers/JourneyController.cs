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

                                     // Fetch the RouteId of the first leg of this journey
                                     routeId = uj.Legs.OrderBy(l => l.SequenceOrder).Select(l => l.RouteId).FirstOrDefault(),

                                     date = uj.CreatedAt.ToString("dd MMM yyyy • HH:mm"),
                                     cost = uj.TotalCost,
                                     routeSummary = $"{startStop.Name} → {endStop.Name}",
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
            // ==========================================
            // DEBUG: Hardcode time to 10:00 AM today
            // ==========================================
            var today = DateTime.UtcNow.Date;
            var currentDateTime = new DateTime(today.Year, today.Month, today.Day, 10, 0, 0, DateTimeKind.Utc);
            //var currentDateTime = DateTime.UtcNow;

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
            // BACKWARD MATH LOGIC
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

            // ==========================================
            // DEBUG: Hardcode time to 10:00 AM today
            // ==========================================
            var today = DateTime.UtcNow.Date;
            //var currentDateTime = DateTime.UtcNow;
            var currentDateTime = new DateTime(today.Year, today.Month, today.Day, 10, 0, 0, DateTimeKind.Utc);

            // 2. Execute the chosen algorithm
            var routeContext = new RoutingContext(request.StartStopId, request.EndStopId, currentDateTime);
            var routingResult = _routingService.ComputeRoute(
                startStopId: request.StartStopId,
                targetStopId: request.EndStopId,
                userDepartureTime: currentDateTime,
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
                currentDateTime.AddMinutes(routingResult.TotalTimeMinutes),
                routingResult.Legs

            );
            //

            _context.UserJourneys.Add(userJourney);
            await _context.SaveChangesAsync();

            // NEW: Write a Ticket row so admin reporting (ticket counts, provider chart) has real data
            var primaryRouteId = routingResult.Legs.FirstOrDefault()?.RouteId;

            var ticket = new Ticket
            {
                QRCodeData = userJourney.TicketReference.ToString(),
                Status = TicketStatus.Active,
                RouteId = primaryRouteId
            };
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            // Extracting unique route IDs for the Firebase subscriptions
            var involvedRouteIds = routingResult.Legs
                .Select(leg => leg.RouteId)
                .Distinct()
                .ToList();

            return Ok(new
            {
                message = "Journey purchased successfully",
                JourneyId = userJourney.JourneyID,
                ticketReference = userJourney.TicketReference,
                totalCost = secureFinalFare,
                estimatedDurationMinutes = routingResult.TotalTimeMinutes,
                estimatedArrivalTime = userJourney.EstimatedArrivalTime.ToString("HH:mm"),
                ticketStatus = userJourney.Status.ToString(),
                routeIds = involvedRouteIds // Add the array to the payload
            });
        }

        //[HttpGet("tickets/{ticketReference}/status")]
        //public async Task<IActionResult> GetTicketStatus(Guid ticketReference)
        //{
        //    var journey = await _context.Journeys
        //        .Include(j => j.Legs)
        //        .FirstOrDefaultAsync(j => j.TicketReference == ticketReference);

        //    if (journey == null)
        //        return NotFound();

        //    // 1. Find routes the passenger HAS ALREADY scanned
        //    var completedRouteIds = journey.Legs
        //        .Where(l => l.RedeemedAt != null)
        //        .Select(l => l.RouteId)
        //        .ToList();

        //    // 2. Find routes they HAVEN'T ridden yet
        //    var upcomingRouteIds = journey.Legs
        //        .Where(l => l.RedeemedAt == null)
        //        .Select(l => l.RouteId)
        //        .ToList();

        //    var expirationWindow = DateTime.UtcNow.AddHours(-2);

        //    // 3. Only check for delays on UPCOMING routes
        //    var activeAlert = await _context.IncidentReports
        //        .Include(i => i.ConductorShift)
        //        .Where(i => upcomingRouteIds.Contains(i.ConductorShift.RouteId)
        //                 && i.Status == IncidentStatus.Open
        //                 && i.Category == IncidentCategory.Delay
        //                 && i.ReportedAt >= expirationWindow)
        //        .OrderByDescending(i => i.ReportedAt)
        //        .Select(i => new
        //        {
        //            message = i.Description,
        //            reportedAt = i.ReportedAt,
        //            isEmergency = i.IsEmergency
        //        })
        //        .FirstOrDefaultAsync();

        //    // 4. Return everything the Android app needs to manage its state
        //    return Ok(new
        //    {
        //        hasDelay = activeAlert != null,
        //        alert = activeAlert,
        //        ticketStatus = journey.Status.ToString().ToUpper(), // "ACTIVE" or "COMPLETED"
        //        completedRouteIds = completedRouteIds // <-- The unsubscription payload!
        //    });
        //}

        [HttpGet("tickets/{ticketReference}/status")]
        public async Task<IActionResult> GetTicketStatus(Guid ticketReference)
        {
            var journey = await _context.Journeys
                .Include(j => j.Legs)
                .FirstOrDefaultAsync(j => j.TicketReference == ticketReference);

            if (journey == null)
                return NotFound();

            // 1. Find routes the passenger HAS ALREADY scanned
            var completedRouteIds = journey.Legs
                .Where(l => l.RedeemedAt != null)
                .Select(l => l.RouteId)
                .ToList();

            // 2. Find routes they HAVEN'T ridden yet
            var upcomingRouteIds = journey.Legs
                .Where(l => l.RedeemedAt == null)
                .Select(l => l.RouteId)
                .ToList();

            var expirationWindow = DateTime.UtcNow.AddHours(-2);

            // 3. Drill through Shift -> Bus to get the physical vehicle details
            var activeAlert = await _context.IncidentReports
                .Include(i => i.ConductorShift)
                    .ThenInclude(cs => cs.Bus) //  Grab the Bus attached to this shift
                .Where(i => upcomingRouteIds.Contains(i.ConductorShift.RouteId)
                         && i.Status == IncidentStatus.Open
                         && i.Category == IncidentCategory.Delay
                         && i.ReportedAt >= expirationWindow)
                .OrderByDescending(i => i.ReportedAt)
                .Select(i => new
                {
                    message = i.Description,
                    reportedAt = i.ReportedAt,
                    isEmergency = i.IsEmergency,
                    busFleetNumber = i.ConductorShift.Bus != null ? i.ConductorShift.Bus.FleetNumber : "Unknown Bus",
                    busLicensePlate = i.ConductorShift.Bus != null ? i.ConductorShift.Bus.LicensePlate : ""
                })
                .FirstOrDefaultAsync();

            // 4. Return everything the Android app needs to manage its state
            return Ok(new
            {
                hasDelay = activeAlert != null,
                alert = activeAlert,
                ticketStatus = journey.Status.ToString().ToUpper(),
                completedRouteIds = completedRouteIds
            });
        }

        [HttpPatch("update-status")]
        [Authorize(Roles = "Conductor, Driver")]
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