//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using System.Security.Claims;
//using UrbanLink_CRUD.Models.Data;
//using UrbanLink_CRUD.Models.Entities.Transit;
//using UrbanLink_CRUD.Services.Routing_Logic;
//using UrbanLink_CRUD.Models.Dtos.TransitDtos;

//namespace UrbanLink_CRUD.Controllers.TransitControllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class JourneyController : ControllerBase
//    {
//        private readonly ULDBContext _context;

//        private readonly TransitSystemService _routingService;


//        public JourneyController(ULDBContext context, TransitSystemService routingService)
//        {
//            _context = context;
//            _routingService = routingService;
//        }

//        [HttpPost("book")]
//        [Authorize]
//        public IActionResult BookJourney([FromBody] BookingRequestDto request)
//        {
//            // Extract Identity
//            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (userIdString == null) return Unauthorized();
//            int userId = int.Parse(userIdString);

//            // --- EXECUTING OUR ALGORITHM ---
//            var routingResult = _routingService.ComputeFastestRoute(request.StartStopId, request.EndStopId);

//            // Handle the "No Route Found" scenario gracefully
//            if (routingResult.TotalTimeMinutes == -1 || routingResult.Legs.Count == 0)
//            {
//                return BadRequest(new { message = "No valid route found between these locations." });
//            }

//            // Since we updated the algorithm to include fares, calculate the total:
//            // (Assuming you added decimal LegFare to your JourneyLeg class in the algorithm)
//            decimal baseTotalCost = routingResult.Legs.Sum(l => l.LegFare);

//            // Apply Student/Pensioner Discounts based on JWT Role
//            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
//            if (userRole == "Student")
//            {
//                baseTotalCost *= 0.85m;
//            }

//            // Build the Master Journey Entity for the database
//            var journey = new Journey
//            {
//                UserId = userId,
//                StartStopId = request.StartStopId,
//                EndStopId = request.EndStopId,
//                TotalCost = baseTotalCost,
//                TotalDurationMinutes = routingResult.TotalTimeMinutes,
//                Status = "Active",
//                CreatedAt = DateTime.UtcNow,
//                Legs = new List<JourneyLeg>()
//            };

//            // Map your algorithm's output to the EF Core Database Entities
//            int sequence = 1;
//            foreach (var algLeg in routingResult.Legs)
//            {
//                journey.Legs.Add(new JourneyLeg
//                {
//                    SequenceOrder = sequence,
//                    RouteId = algLeg.RouteId,
//                    BoardingStopId = algLeg.StartStopId,
//                    DestinationStopId = algLeg.EndStopId,
//                    LegDurationMinutes = algLeg.LegDurationMinutes,
//                    LegFare = algLeg.LegFare,
//                    Status = "Active",
//                    TicketReference = Guid.NewGuid()
//                });
//                sequence++;
//            }

//            // Save transaction
//            _context.Journeys.Add(journey);
//            _context.SaveChanges();

//            return Ok(new
//            {
//                message = "Journey booked successfully.",
//                journeyId = journey.Id,
//                totalCost = journey.TotalCost,
//                estimatedDurationMinutes = journey.TotalDurationMinutes
//            });
//        }
//    }
//}

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
                totalCost = journey.TotalCost,
                estimatedDurationMinutes = journey.TotalDurationMinutes
            });
        }

    }
}