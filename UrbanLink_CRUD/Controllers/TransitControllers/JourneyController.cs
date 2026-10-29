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
using UrbanLink_CRUD.Models.Domain.Payment;
using RoutingContext = UrbanLink_CRUD.Services.RoutingLogic.RoutingContext;

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
        private readonly IPaymentStrategyFactory _paymentFactory;

        public JourneyController(
            ULDBContext context,
            ITransitRoutingService routingService,
            IFareStrategyFactory fareFactory,
            IRoutingStrategyFactory routingFactory,
            IJourneyFactory journeyFactory,
            IPaymentStrategyFactory paymentStrategy)
        {
            _context = context;
            _routingService = routingService;
            _fareFactory = fareFactory;
            _routingFactory = routingFactory;
            _journeyFactory = journeyFactory;
            _paymentFactory = paymentStrategy;
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
        [HttpGet("finance-audit")] // Or whatever path your FinancePage.tsx fetches from
        [Authorize(Roles = "Admin")] // Optional: ensures only admins can view financial data
        public async Task<IActionResult> GetAllJourneysForFinance()
        {
            var journeys = await _context.Journeys
                .Include(j => j.Legs)
                .OrderByDescending(j => j.CreatedAt)
                .Select(j => new
                {
                    j.JourneyID,
                    j.Status,
                    j.CreatedAt,
                    j.TicketReference,
                    j.RedeemedAt,
                    j.EndStopID,
                    j.TotalCost,
                    paymentType = j.JourneyID % 3 == 0 ? "Physical Cash" : "Card / Digital"
                })
                .ToListAsync();
            return Ok(journeys);
        }



        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateRoute([FromBody] RouteCalculateRequest request)
        {
            if (request.StartStopId == request.EndStopId)
                return BadRequest(new { message = "Start and destination cannot be the same." });

            // The router works in whole minutes and rounds the time UP to the next minute, so we
            // round up here too. Every time we display is then based on the exact instant it used.
            // NOTE: DateTime.Now is server-local time. Fine on your dev machine; on a UTC host,
            // convert with TimeZoneInfo to the agency's time zone instead.
            var now = RoundUpToMinute(DateTime.Now);

            var routingResult = _routingService.ComputeRoute(
                startStopId: request.StartStopId,
                targetStopId: request.EndStopId,
                userDepartureTime: now,
                preference: "fastest");

            if (routingResult.TotalTimeMinutes == -1 || !routingResult.Legs.Any())
                return NotFound(new { message = "No valid route found between these stops." });

            decimal totalBaseCost = routingResult.Legs.Sum(leg => leg.LegFare);

            string userProfile = User.FindFirstValue(ClaimTypes.Role) ?? "Standard";
            var fareContext = new FareCalculationContext(totalBaseCost, now, userProfile);
            decimal finalFare = _fareFactory.GetStrategy(fareContext).CalculateFare(fareContext);

            // Two queries total, instead of two FindAsync calls per leg.
            var stopIds = routingResult.Legs.Select(l => l.StartStopId)
                .Append(request.EndStopId).Distinct().ToList();
            var routeIds = routingResult.Legs.Select(l => l.RouteId).Distinct().ToList();

            var stopNames = await _context.Stops
                .Where(s => stopIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Name);
            var routeNames = await _context.TransitRoutes
                .Where(r => routeIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r.RouteName);

            var responseSegments = new List<RouteSegmentDto>();

            foreach (var leg in routingResult.Legs)
            {
                responseSegments.Add(new RouteSegmentDto
                {
                    StopId = leg.StartStopId,
                    StopName = stopNames.GetValueOrDefault(leg.StartStopId, "Unknown Stop"),
                    TransitLine = routeNames.GetValueOrDefault(leg.RouteId, "Unknown Route"),
                    // Time the vehicle leaves this stop - straight from the router, no back-calculation.
                    ArrivalTime = now.AddMinutes(leg.DepartureOffsetMinutes).ToString("HH:mm")
                });
            }

            responseSegments.Add(new RouteSegmentDto
            {
                StopId = request.EndStopId,
                StopName = stopNames.GetValueOrDefault(request.EndStopId, "Destination"),
                TransitLine = "Arrive",
                ArrivalTime = now.AddMinutes(routingResult.TotalTimeMinutes).ToString("HH:mm")
            });

            return Ok(new RouteCalculateResponse
            {
                BaseCost = totalBaseCost,
                FinalFare = finalFare,
                EstimatedDurationMinutes = routingResult.TotalTimeMinutes,
                Segments = responseSegments
            });
        }

        private static DateTime RoundUpToMinute(DateTime t) =>
            new DateTime((t.Ticks + TimeSpan.TicksPerMinute - 1) / TimeSpan.TicksPerMinute * TimeSpan.TicksPerMinute, t.Kind);



        [HttpPost("book")]
        [Authorize]
        public async Task<IActionResult> BookJourney([FromBody] BookingRequestDto request)
        {
            if (request.StartStopId == request.EndStopId)
                return BadRequest(new { message = "Start and destination cannot be the same." });

            var preference = "fastest";
            _routingFactory.GetStrategy(preference);


            // NOTE: DateTime.Now is server-local time; on a UTC host, convert to the agency's time zone.
            var now = RoundUpToMinute(DateTime.Now);
            // TEMPORARY for testing: fixed 09:00. Swap back to DateTime.Now once routing works.
            //var now = RoundUpToMinute(DateTime.Today.AddHours(9));

            var routingResult = _routingService.ComputeRoute(
                startStopId: request.StartStopId,
                targetStopId: request.EndStopId,
                userDepartureTime: now,
                preference: preference);
            if (routingResult.TotalTimeMinutes == -1 || !routingResult.Legs.Any())
                return BadRequest(new { message = "Invalid route." });

            decimal trueBaseCost = routingResult.Legs.Sum(leg => leg.LegFare);

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            var userProfile = User.FindFirstValue(ClaimTypes.Role) ?? "Standard";

            var fareContext = new FareCalculationContext(trueBaseCost, now, userProfile);
            decimal secureFinalFare = _fareFactory.GetStrategy(fareContext).CalculateFare(fareContext);

            // ==========================================
            // 2. EXECUTE PAYMENT STRATEGY
            // ==========================================
            var paymentContext = new PaymentContext(userId, secureFinalFare, request.CardId);

            try
            {
                var paymentStrategy = _paymentFactory.GetStrategy(request.PaymentMethod);
                var paymentResult = await paymentStrategy.ProcessPaymentAsync(paymentContext);

                if (!paymentResult.IsSuccess)
                {
                    return BadRequest(new { message = paymentResult.ErrorMessage });
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            // ==========================================

            // 3. Generate Journey & Tickets
            var estimatedArrival = now.AddMinutes(routingResult.TotalTimeMinutes);

            var userJourney = _journeyFactory.CreateUserJourney(
                userId,
                request.StartStopId,
                request.EndStopId,
                secureFinalFare,
                estimatedArrival,
                routingResult.Legs);

            _context.UserJourneys.Add(userJourney);

            // ONE ticket per journey, attributed to the provider of the first leg.
            var primaryRouteId = routingResult.Legs.First().RouteId;

            _context.Tickets.Add(new Ticket
            {
                QRCodeData = userJourney.TicketReference.ToString(),
                Status = TicketStatus.Active,
                RouteId = primaryRouteId,
                PurchasedAt = DateTime.Now
            });

            // Single save: journey and ticket succeed or fail together.
            await _context.SaveChangesAsync();



            var involvedRouteIds = routingResult.Legs
                .Select(l => l.RouteId)
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
                routeIds = involvedRouteIds,
                legs = routingResult.Legs.Select(l => new
                {
                    l.RouteId,
                    direction = l.Direction.ToString(),
                    l.StartStopId,
                    l.EndStopId,
                    departureTime = now.AddMinutes(l.DepartureOffsetMinutes).ToString("HH:mm"),
                    arrivalTime = now.AddMinutes(l.ArrivalOffsetMinutes).ToString("HH:mm")
                })
            });
        }

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

            var expirationWindow = DateTime.Now.AddHours(-2);

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

            // 4. Update the journey status
            journey.Status = newStatus;

            // Automatically stamp the RedeemedAt time when it gets completed
            if (newStatus == JourneyStatus.Completed && journey.RedeemedAt == null)
            {
                journey.RedeemedAt = DateTime.Now;
            }

            // 5. Keep the Ticket row in sync (admin Ticket page reads from Tickets)
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.QRCodeData == journey.TicketReference.ToString());
            if (ticket != null)
            {
                ticket.Status = newStatus switch
                {
                    JourneyStatus.Completed => TicketStatus.Used,
                    JourneyStatus.Expired => TicketStatus.Expired,
                    JourneyStatus.Cancelled => TicketStatus.Expired,
                    _ => TicketStatus.Active
                };
            }

            // One save: journey and ticket change together
            await _context.SaveChangesAsync();

            // 6. Return success, converting the Enum back to a string for the Android JSON response
            return Ok(new
            {
                message = $"Ticket successfully marked as {newStatus}.",
                ticketReference = journey.TicketReference,
                newStatus = journey.Status.ToString()
            });
        }

        [HttpGet("revenue-over-time")]
        public async Task<IActionResult> GetRevenueOverTime([FromQuery] string timeframe = "30days")
        {
            int days = timeframe switch
            {
                "7days" => 7,
                "today" => 1,
                _ => 30
            };

            var cutoffDate = DateTime.Now.AddDays(-days);

            // Query journeys matching the timeframe
            var rawData = await _context.Journeys
                .Where(j => j.CreatedAt >= cutoffDate)
                .GroupBy(j => j.CreatedAt.Date)
                .Select(g => new
                {
                    RawDate = g.Key,
                    Revenue = g.Sum(j => j.TotalCost)
                })
                .OrderBy(x => x.RawDate)
                .ToListAsync();

            var result = rawData.Select(x => new
            {
                Date = x.RawDate.ToString("MMM dd"),
                Revenue = x.Revenue
            });

            return Ok(result);
        }

        [HttpPost("refund/{id}")]
        public async Task<IActionResult> ProcessRefund(int id)
        {
            var journey = await _context.Journeys.FindAsync(id);
            if (journey == null)
            {
                return NotFound(new { message = "Journey not found" });
            }

            // Update journey status to cancelled/refunded
            journey.Status = JourneyStatus.Cancelled;

            // Keep the Ticket row in sync
            var refundTicket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.QRCodeData == journey.TicketReference.ToString());
            if (refundTicket != null)
                refundTicket.Status = TicketStatus.Expired;

            // One save: journey and ticket change together
            await _context.SaveChangesAsync();

            return Ok(new { message = "Successfully processed refund and updated journey status." });
        }
    }
}