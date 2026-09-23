using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Domain;
using UrbanLink_CRUD.Models.Domain.Journey;
using UrbanLink_CRUD.Models.Dtos.TransitDtos;
using UrbanLink_CRUD.Models.Entities.Transit;
using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Controllers.TransitControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConductorShiftController : ControllerBase
    {
        private readonly ULDBContext _context;
        private readonly IJourneyFactory _journeyFactory;
        private readonly IHubContext<TransitHub> _hubContext;

        public ConductorShiftController(ULDBContext context, IJourneyFactory journeyFactory, IHubContext<TransitHub> hubContext)
        {
            _context = context;
            _journeyFactory = journeyFactory;
            _hubContext = hubContext;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartShift([FromBody] StartShiftRequest request)
        {
            var conductorIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "Conductor";

            if (string.IsNullOrEmpty(conductorIdString))
                return Unauthorized(new { message = "Invalid user token." });

            int conductorId = int.Parse(conductorIdString);

            // 1. Check if this user already has an active shift anywhere
            var activeShift = await _context.ConductorShifts
                .FirstOrDefaultAsync(cs => cs.ConductorId == conductorId && cs.ShiftEndTime == null);

            if (activeShift != null)
                return BadRequest(new { message = "You already have an active shift.", shiftId = activeShift.Id });

            // 2. Check if the bus and route exist
            var busExists = await _context.Buses.AnyAsync(b => b.Id == request.BusId);
            var routeExists = await _context.TransitRoutes.AnyAsync(r => r.Id == request.RouteId);

            if (!busExists || !routeExists)
                return NotFound(new { message = "The selected Bus or Route does not exist." });

            // 3. ROLE-BASED BUS CONFLICT CHECK:
            // Ensure the bus doesn't already have someone logged in with the exact same role
            var existingBusShifts = await _context.ConductorShifts
                .Where(cs => cs.BusId == request.BusId && cs.ShiftEndTime == null)
                .Include(cs => cs.Conductor)
                .ToListAsync();

            //bool roleAlreadyExistsOnBus = existingBusShifts.Any(cs => cs.Conductor?.Role.Equals(userRole, StringComparison.OrdinalIgnoreCase) == true);

            //Role userRole = Enum.TryParse<Role>(User.FindFirstValue(ClaimTypes.Role), out var parsedRole) ? parsedRole : Role.Conductor;
            Role parsedUserRole = Enum.TryParse<Role>(userRole, out var r) ? r : Role.Conductor;
            bool roleAlreadyExistsOnBus = existingBusShifts.Any(cs => cs.Conductor?.Role == parsedUserRole);

            if (roleAlreadyExistsOnBus)
            {
                return BadRequest(new { message = $"This bus already has an active {userRole} assigned." });
            }

            // 4. Create the new shift
            var newShift = new ConductorShift
            {
                ConductorId = conductorId,
                BusId = request.BusId,
                RouteId = request.RouteId,
                ShiftStartTime = DateTime.UtcNow,
                TotalCashCollected = 0,
                DigitalTicketScanned = 0
            };

            _context.ConductorShifts.Add(newShift);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Shift started successfully.", shiftId = newShift.Id, startTime = newShift.ShiftStartTime });
        }

        [HttpPost("scan")]
        [Authorize(Roles = "Conductor, Driver")]
        public async Task<IActionResult> ScanTicket([FromBody] ScanTicketRequest request)
        {
            if (!Guid.TryParse(request.QRCodeData, out Guid ticketGuid))
                return BadRequest(new { message = "Invalid QR Code format." });

            // 1. Extract the authenticated conductor's ID
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int conductorId))
                return Unauthorized(new { message = "Invalid token or user ID." });

            // 2. Find the active shift
            var shift = await _context.ConductorShifts
                .FirstOrDefaultAsync(s => s.ConductorId == conductorId && s.ShiftEndTime == null);

            if (shift == null)
                return BadRequest(new { message = "No active shift found. Please start a shift first." });

            // 3. Load the Journey AND its associated Legs
            var journey = await _context.Journeys
                .Include(j => j.Legs)
                .FirstOrDefaultAsync(j => j.TicketReference == ticketGuid);

            if (journey == null)
                return NotFound(new { message = "Invalid Ticket: Not found in system." });

            if (journey.Status != JourneyStatus.Active)
                return BadRequest(new { message = $"Ticket Rejected: Already {journey.Status}." });

            // 4. Find the specific JourneyLeg that matches the Conductor's Route
            var currentLeg = journey.Legs.FirstOrDefault(l => l.RouteId == shift.RouteId);

            if (currentLeg == null)
                return BadRequest(new { message = "Ticket Rejected: Not valid for this specific route." });

            if (currentLeg.RedeemedAt != null)
                return BadRequest(new { message = "Ticket Rejected: Already scanned for this route." });

            // 5. CASCADE VOID (The Skipping Logic)
            // Find any legs that came BEFORE this one that the passenger skipped, and void them.
            var skippedLegs = journey.Legs
                .Where(l => l.SequenceOrder < currentLeg.SequenceOrder && l.RedeemedAt == null)
                .ToList();

            foreach (var skipped in skippedLegs)
            {
                // Stamping them with the current time effectively consumes them so they cannot be reused
                skipped.RedeemedAt = DateTime.UtcNow;
            }

            // 6. Redeem the current leg
            currentLeg.RedeemedAt = DateTime.UtcNow;
            shift.DigitalTicketScanned += 1;

            // 7. Check if this is the final leg of the journey
            bool isFinalLeg = !journey.Legs.Any(l => l.SequenceOrder > currentLeg.SequenceOrder);

            if (isFinalLeg)
            {
                journey.Status = JourneyStatus.Completed;
                journey.RedeemedAt = DateTime.UtcNow;
            }

            _context.Journeys.Update(journey);
            _context.ConductorShifts.Update(shift);
            await _context.SaveChangesAsync();

            // 8. QUEUE LOGIC: Use the Destination Stop of the CURRENT LEG, not the parent Journey
            var destinationStopId = currentLeg.DestinationStopId;

            var activeStop = await _context.TripDestinations
                .FirstOrDefaultAsync(td => td.ShiftId == shift.Id && td.StopId == destinationStopId);

            if (activeStop == null)
            {
                _context.TripDestinations.Add(new TripDestination
                {
                    ShiftId = shift.Id,
                    StopId = destinationStopId,
                    Status = "QUEUED",
                    PassengersDroppingOff = 1 // Initialize drop-off count
                });
            }
            else
            {
                // If it exists, increment the number of passengers dropping off here
                activeStop.PassengersDroppingOff += 1;
                _context.TripDestinations.Update(activeStop);
            }

            await _context.SaveChangesAsync();

            // 9. Fetch the updated, sequence-sorted list for the UI
            var updatedQueue = await _context.TripDestinations
                .Where(td => td.ShiftId == shift.Id && td.Status != "CLEARED")
                .Join(
                    _context.RouteStops,
                    td => td.StopId,
                    rs => rs.StopId,
                    (td, rs) => new { td, rs }
                )
                .Join(
                    _context.RouteSegments,
                    combined => combined.rs.SegmentId,
                    seg => seg.Id,
                    (combined, seg) => new { combined.td, combined.rs, seg }
                )
                .Where(x => x.seg.RouteId == shift.RouteId)
                .OrderBy(x => x.rs.StopSequence)
                .Select(x => new
                {
                    Name = x.td.Stop.Name,
                    SequenceNumber = x.rs.StopSequence,
                    PassengersDroppingOff = x.td.PassengersDroppingOff
                })
                .ToListAsync();

            // 10. OBSERVER TRIGGER: Broadcast the new queue
            string jsonQueue = System.Text.Json.JsonSerializer.Serialize(updatedQueue);
            string targetGroup = $"bus_{shift.BusId}";

            Console.WriteLine($"[SignalR] Broadcasting 'QueueUpdated' to group: {targetGroup}");
            await _hubContext.Clients.Group(targetGroup).SendAsync("QueueUpdated", jsonQueue);

            // 11. Return success payload to Conductor App
            return Ok(new
            {
                message = skippedLegs.Any() ? "Valid (Previous Leg Forfeited)" : "Ticket Validated",
                fareAmount = currentLeg.LegFare, // Changed to show the fare for THIS leg, not the total
                activeQueue = updatedQueue
            });
        }

        //[HttpPost("scan")]
        //[Authorize(Roles = "Conductor, Driver")]
        //public async Task<IActionResult> ScanTicket([FromBody] ScanTicketRequest request)
        //{
        //    if (!Guid.TryParse(request.QRCodeData, out Guid ticketGuid))
        //        return BadRequest(new { message = "Invalid QR Code format." });

        //    // 1. Extract the authenticated conductor's ID from the JWT token claims
        //    var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        //    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int conductorId))
        //        return Unauthorized(new { message = "Invalid token or user ID." });

        //    // 2. Automatically find the active shift for this specific conductor on the backend
        //    var shift = await _context.ConductorShifts
        //        .FirstOrDefaultAsync(s => s.ConductorId == conductorId && s.ShiftEndTime == null);

        //    if (shift == null)
        //        return BadRequest(new { message = "No active shift found. Please start a shift first." });

        //    var journey = await _context.Journeys.FirstOrDefaultAsync(j => j.TicketReference == ticketGuid);

        //    if (journey == null)
        //        return NotFound(new { message = "Invalid Ticket: Not found in system." });

        //    // Uses strong Enum
        //    if (journey.Status != JourneyStatus.Active)
        //        return BadRequest(new { message = $"Ticket Rejected: Already {journey.Status}." });

        //    // Uses strong Enum
        //    journey.Status = JourneyStatus.Completed;
        //    journey.RedeemedAt = DateTime.UtcNow;

        //    // Increment the counter directly on the shift for the summary screen
        //    shift.DigitalTicketScanned += 1;

        //    _context.Journeys.Update(journey);
        //    _context.ConductorShifts.Update(shift);
        //    await _context.SaveChangesAsync();

        //    // 1. Identify the destination stop from the scanned journey
        //    var destinationStopId = journey.EndStopID; // Assuming this exists on Journey

        //    // 2. Add to active queue if it doesn't exist
        //    var activeStop = await _context.TripDestinations
        //        .FirstOrDefaultAsync(td => td.ShiftId == shift.Id && td.StopId == destinationStopId);

        //    if (activeStop == null)
        //    {
        //        _context.TripDestinations.Add(new TripDestination
        //        {
        //            ShiftId = shift.Id,
        //            StopId = destinationStopId,
        //            Status = "QUEUED"
        //        });
        //    }

        //    await _context.SaveChangesAsync();

        //    // 3. Fetch the updated, sequence-sorted list
        //    var updatedQueue = await _context.TripDestinations
        //        .Where(td => td.ShiftId == shift.Id && td.Status != "CLEARED")
        //        .Join(
        //            _context.RouteStops,
        //            td => td.StopId,
        //            rs => rs.StopId,
        //            (td, rs) => new { td, rs }
        //        )
        //        .Join(
        //            _context.RouteSegments,
        //            combined => combined.rs.SegmentId,
        //            seg => seg.Id,
        //            (combined, seg) => new { combined.td, combined.rs, seg }
        //        )
        //        // Safely filter by the RouteId since stops do not duplicate on SA routes
        //        .Where(x => x.seg.RouteId == shift.RouteId)
        //        .OrderBy(x => x.rs.StopSequence)
        //        .Select(x => new
        //        {
        //            Name = x.td.Stop.Name,
        //            SequenceNumber = x.rs.StopSequence,
        //            PassengersDroppingOff = x.td.PassengersDroppingOff
        //        })
        //        .ToListAsync();

        //    // OBSERVER TRIGGER: Broadcast the new queue to the specific shift group
        //    string jsonQueue = System.Text.Json.JsonSerializer.Serialize(updatedQueue);

        //    // --- ADD THIS LINE TO CHECK SERVER CONSOLE ---
        //    string targetGroup = $"bus_{shift.BusId}";
        //    Console.WriteLine($"[SignalR] Broadcasting 'QueueUpdated' to group: {targetGroup}");

        //    // Broadcast the string to the bus group
        //    await _hubContext.Clients.Group(targetGroup)
        //    .SendAsync("QueueUpdated", jsonQueue);

        //    // 4. Return the new queue to the Android app instantly
        //    return Ok(new
        //    {
        //        message = "Ticket Validated",
        //        fareAmount = journey.TotalCost,
        //        activeQueue = updatedQueue // The Android app uses this to redraw the UI
        //    });
        //}

        [HttpPost("cash-sale")]
        [Authorize(Roles = "Conductor, Driver")]
        public async Task<IActionResult> RecordCashSale([FromBody] CashSaleRequest request)
        {
            // 1. Extract the authenticated conductor's ID from the JWT token claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int conductorId))
                return Unauthorized(new { message = "Invalid token or user ID." });

            // 2. Automatically find the active shift for this specific conductor on the backend
            var shift = await _context.ConductorShifts
                .Include(s => s.Route)
                .FirstOrDefaultAsync(s => s.ConductorId == conductorId && s.ShiftEndTime == null);

            if (shift == null)
                return BadRequest(new { message = "Invalid or closed shift. No active shift found." });

            // 3. Find the Terminal (Last) Stop of this specific Route
            var terminalSegment = await _context.RouteSegments
                .FirstOrDefaultAsync(rs => rs.RouteId == shift.RouteId &&
                    !_context.RouteSegments.Any(other => other.RouteId == shift.RouteId && other.OriginStopId == rs.DestinationStopId));

            if (terminalSegment == null)
                return BadRequest(new { message = "Route configuration error: Cannot determine terminal stop." });

            int endStopId = terminalSegment.DestinationStopId;

            // 4. Calculate Fares
            decimal fareTotal = shift.Route.BaseFare * request.PassengerCount;
            shift.TotalCashCollected += fareTotal;

            var issuedTickets = new List<PrintedTicketDto>();

            // 5. Issue the tickets for the waist printer
            for (int i = 0; i < request.PassengerCount; i++)
            {
                var walkOnJourney = _journeyFactory.CreateCashJourney(shift.Id, shift.Route.BaseFare, endStopId);

                string displayTicketId = $"CASH-{walkOnJourney.TicketReference.ToString().Substring(0, 8).ToUpper()}";

                issuedTickets.Add(new PrintedTicketDto
                {
                    TicketId = displayTicketId,
                    RouteName = shift.Route.RouteName,
                    FarePaid = shift.Route.BaseFare,
                    IssuedAt = walkOnJourney.RedeemedAt.Value
                });
            }

            _context.ConductorShifts.Update(shift);
            await _context.SaveChangesAsync();

            // 6. Build the active queue using RouteStop sequence numbers and Stop names
            var activeQueue = await _context.RouteStops
                .Where(rs => rs.RouteSegment.RouteId == shift.RouteId)
                .Join(_context.Stops,
                      rs => rs.StopId,
                      s => s.Id,
                      (rs, s) => new { RouteStop = rs, Stop = s })
                .GroupJoin(_context.Journeys.Where(j => j.Status == JourneyStatus.Active),
                           x => x.Stop.Id,
                           j => j.EndStopID,
                           (x, journeys) => new
                           {
                               Name = x.Stop.Name,
                               SequenceNumber = x.RouteStop.StopSequence,
                               PassengersDroppingOff = journeys.Count()
                           })
                // Optional: Only include stops that have active drop-offs or are part of the active queue sequence
                .Where(q => q.PassengersDroppingOff > 0)
                .OrderBy(q => q.SequenceNumber)
                .ToListAsync();

            // 7. Broadcast real-time queue update to the driver via SignalR
            await _hubContext.Clients.Group($"bus_{shift.BusId}")
                .SendAsync("QueueUpdated", System.Text.Json.JsonSerializer.Serialize(activeQueue));

            return Ok(new
            {
                message = "Cash sale recorded successfully.",
                amountAdded = fareTotal,
                newTotal = shift.TotalCashCollected,
                receipts = issuedTickets,
                activeQueue = activeQueue
            });
        }

        [HttpGet("summary")]
        [Authorize(Roles = "Conductor, Driver, Admin")]
        public async Task<IActionResult> GetShiftSummary()
        {
            var conductorIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(conductorIdString)) return Unauthorized();
            int conductorId = int.Parse(conductorIdString);

            var activeShift = await _context.ConductorShifts
                .Include(cs => cs.Bus)
                .Include(cs => cs.Route)
                .FirstOrDefaultAsync(cs => cs.ConductorId == conductorId && cs.ShiftEndTime == null);

            if (activeShift == null)
                return BadRequest(new { message = "No active shift found." });

            return Ok(new
            {
                message = "Live summary fetched.",
                shiftSummary = new
                {
                    shiftId = activeShift.Id,
                    busFleetNumber = activeShift.Bus.FleetNumber,
                    routeName = activeShift.Route.RouteName,
                    startTime = activeShift.ShiftStartTime,
                    totalCashHandover = activeShift.TotalCashCollected,
                    digitalTicketsScanned = activeShift.DigitalTicketScanned // Reads direct property
                }
            });
        }

        [HttpPost("end")]
        [Authorize(Roles = "Conductor, Driver, Admin")]
        public async Task<IActionResult> EndShift()
        {
            var conductorIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(conductorIdString)) return Unauthorized();
            int conductorId = int.Parse(conductorIdString);

            var activeShift = await _context.ConductorShifts
                .Include(cs => cs.Bus)
                .Include(cs => cs.Route)
                .FirstOrDefaultAsync(cs => cs.ConductorId == conductorId && cs.ShiftEndTime == null);

            if (activeShift == null)
                return BadRequest(new { message = "No active shift found. You are already clocked out." });

            activeShift.ShiftEndTime = DateTime.UtcNow;

            _context.ConductorShifts.Update(activeShift);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Shift ended successfully.",
                shiftSummary = new
                {
                    shiftId = activeShift.Id,
                    busFleetNumber = activeShift.Bus.FleetNumber,
                    routeName = activeShift.Route.RouteName,
                    startTime = activeShift.ShiftStartTime,
                    endTime = activeShift.ShiftEndTime,
                    totalCashHandover = activeShift.TotalCashCollected,
                    digitalTicketsScanned = activeShift.DigitalTicketScanned // Reads direct property
                }
            });
        }


        [HttpPost("admin/force-logout-crew")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ForceLogoutAllCrew()
        {
            // 1. Find all active shifts belonging to Drivers or Conductors
            var activeCrewShifts = await _context.ConductorShifts
                .Include(cs => cs.Conductor)
                .Where(cs => cs.ShiftEndTime == null &&
                       (cs.Conductor.Role == Role.Driver || cs.Conductor.Role == Role.Conductor))
                .ToListAsync();

            if (!activeCrewShifts.Any())
            {
                return Ok(new { message = "No active crew shifts found to terminate." });
            }

            // 2. Terminate all active shifts
            foreach (var shift in activeCrewShifts)
            {
                shift.ShiftEndTime = DateTime.UtcNow;
            }

            _context.ConductorShifts.UpdateRange(activeCrewShifts);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Successfully terminated shifts for {activeCrewShifts.Count} crew members.",
                terminatedShiftIds = activeCrewShifts.Select(s => s.Id).ToList()
            });
        }
    }
}
