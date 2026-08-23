using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Timers;
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
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null) return Unauthorized();
            int userId = int.Parse(userIdString);

            // 2. Query the database using Entity Framework
            var history = await _context.Journeys
                // Include the physical Stops so we can access their Names
                .Include(j => j.StartStop)
                .Include(j => j.EndStop)
                .Where(j => j.UserId == userId)
                .OrderByDescending(j => j.CreatedAt) // Newest trips at the top!
                .Select(j => new
                {
                    ticketReference = j.TicketReference,

                    // Format the date exactly how your Android XML expects it
                    date = j.CreatedAt.ToString("dd MMM yyyy • HH:mm"),

                    // Raw number, we can append the 'R' on the Android side
                    cost = j.TotalCost,

                    // Dynamically build the "A -> B" string
                    routeSummary = $"{j.StartStop.Name} → {j.EndStop.Name}",

                    // Force uppercase to match your XML design ("REDEEMED", "ACTIVE")
                    status = j.Status.ToUpper()
                })
                .ToListAsync();

            // 3. Return the array to the app!
            // If they have no trips, this safely returns an empty array [] instead of a 404
            return Ok(history);
        }


        // Calculate route before booking
        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateRoute([FromBody] RouteCalculateRequest request)
        {
            if (request.StartStopId == request.EndStopId)
                return BadRequest(new { message = "Start and destination cannot be the same." });

            // 1. Get the exact current DateTime for the user
            // var currentDateTime = DateTime.Now;
            var currentDateTime = new DateTime(2026, 8, 24, 10, 0, 0);

            // 2. Run your fantastic routing engine using the FULL DateTime context
            var routingResult = _routingService.ComputeFastestRoute(request.StartStopId, request.EndStopId, currentDateTime);

            // -1 indicates no path was found
            if (routingResult.TotalTimeMinutes == -1 || !routingResult.Legs.Any())
                return NotFound(new { message = "No valid route found between these stops." });

            // 3. Tally up the total base cost from all the legs
            decimal totalBaseCost = routingResult.Legs.Sum(leg => leg.LegFare);

            var responseSegments = new List<RouteSegmentDto>();

            // ==========================================
            // BACKWARD MATH LOGIC
            // ==========================================
            // A. Calculate the perfect final arrival time (Current Time + Total Dijkstra Cost)
            var finalArrivalTime = currentDateTime.AddMinutes(routingResult.TotalTimeMinutes);

            // B. Sum up the physical travel time (time actually spent moving on buses)
            int physicalTravelTime = routingResult.Legs.Sum(l => l.LegDurationMinutes);

            // C. Calculate transfer penalties (10 mins per transfer based on your algorithm)
            int totalTransfers = routingResult.Legs.Count - 1;
            int transferPenalties = totalTransfers > 0 ? (totalTransfers * 10) : 0;

            // D. Work backward to find the EXACT time they step onto the first bus!
            var currentLegBoardingTime = finalArrivalTime.AddMinutes(-(physicalTravelTime + transferPenalties));

            // 4. Translate the raw IDs into human-readable data for Android
            foreach (var leg in routingResult.Legs)
            {
                // Fetch the actual names from the database
                var startStop = await _context.Stops.FindAsync(leg.StartStopId);
                var routeInfo = await _context.TransitRoutes.FindAsync(leg.RouteId);

                // Calculate when they get off THIS specific bus
                var legArrivalTime = currentLegBoardingTime.AddMinutes(leg.LegDurationMinutes);

                responseSegments.Add(new RouteSegmentDto
                {
                    StopId = leg.StartStopId,
                    StopName = startStop?.Name ?? "Unknown Stop",
                    TransitLine = routeInfo?.RouteName ?? "Unknown Route",
                    // This shows the time they board this specific segment
                    ArrivalTime = currentLegBoardingTime.ToString("HH:mm")
                });

                // Setup the boarding time for the NEXT leg (leg arrival + 10 min transfer penalty)
                currentLegBoardingTime = legArrivalTime.AddMinutes(10);
            }

            // 5. Add the final destination node to the end of the timeline
            var finalStop = await _context.Stops.FindAsync(request.EndStopId);
            responseSegments.Add(new RouteSegmentDto
            {
                StopId = request.EndStopId,
                StopName = finalStop?.Name ?? "Destination",
                TransitLine = "Arrive",
                ArrivalTime = finalArrivalTime.ToString("HH:mm")
            });

            // 6. Package it all up for Android!
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

            // 1. Get the current full DateTime for the user
            // var currentDateTime = DateTime.Now;
            var currentDateTime = new DateTime(2026, 8, 24, 10, 0, 0);

            // --- EXECUTING OUR ALGORITHM ---
            // Pass the full currentDateTime instead of just the TimeOfDay
            var routingResult = _routingService.ComputeFastestRoute(request.StartStopId, request.EndStopId, currentDateTime);

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

            // A. Calculate the perfect final arrival time
            var finalArrivalTime = currentDateTime.AddMinutes(routingResult.TotalTimeMinutes);

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
                estimatedDurationMinutes = journey.TotalDurationMinutes,
                estimatedArrivalTime = finalArrivalTime.ToString("HH:mm")
            });
        }

        [HttpPatch("update-status")]
        [Authorize]
        public async Task<IActionResult> UpdateTicketStatus([FromBody] UpdateTicketStatusDto request)
        {
            // 1. Force the status to uppercase so it perfectly matches your Android UI logic
            string newStatus = request.Status.ToUpper();

            // 2. Validate that the status is one of our allowed keywords
            var validStatuses = new[] { "ACTIVE", "REDEEMED", "EXPIRED" };
            if (!validStatuses.Contains(newStatus))
            {
                return BadRequest(new { message = "Invalid status. Must be ACTIVE, REDEEMED, or EXPIRED." });
            }

            // 3. Find the Journey (Ticket) in the database using the unique GUID
            var journey = await _context.Journeys
                .FirstOrDefaultAsync(j => j.TicketReference == request.TicketReference);

            if (journey == null)
            {
                return NotFound(new { message = "Ticket not found." });
            }

            // 4. Security Check: Prevent double-scanning!
            if (journey.Status == "REDEEMED" && newStatus == "REDEEMED")
            {
                return BadRequest(new { message = "Ticket has already been redeemed! Do not let passenger board." });
            }

            if (journey.Status == "EXPIRED")
            {
                return BadRequest(new { message = "Ticket is expired and no longer valid." });
            }

            // 5. Update the status and save to SQL Server
            journey.Status = newStatus;

            await _context.SaveChangesAsync();

            // 6. Return success
            return Ok(new
            {
                message = $"Ticket successfully marked as {newStatus}.",
                ticketReference = journey.TicketReference,
                newStatus = journey.Status
            });
        }
    }
}