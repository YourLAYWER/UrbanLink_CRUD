using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UrbanLink_CRUD.Models.Data;
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

        public ConductorShiftController(ULDBContext context, IJourneyFactory journeyFactory)
        {
            _context = context;
            _journeyFactory = journeyFactory;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartShift([FromBody] StartShiftRequest request)
        {
            var conductorIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(conductorIdString)) return Unauthorized(new { message = "Invalid user token." });

            int conductorId = int.Parse(conductorIdString);

            var activeShift = await _context.ConductorShifts
                .FirstOrDefaultAsync(cs => cs.ConductorId == conductorId && cs.ShiftEndTime == null);

            if (activeShift != null)
                return BadRequest(new { message = "You already have an active shift.", shiftId = activeShift.Id });

            var busExists = await _context.Buses.AnyAsync(b => b.Id == request.BusId);
            var routeExists = await _context.TransitRoutes.AnyAsync(r => r.Id == request.RouteId);

            if (!busExists || !routeExists)
                return NotFound(new { message = "The selected Bus or Route does not exist." });

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
        public async Task<IActionResult> ScanTicket([FromBody] ScanTicketRequest request)
        {
            if (!Guid.TryParse(request.QRCodeData, out Guid ticketGuid))
                return BadRequest(new { message = "Invalid QR Code format." });

            var journey = await _context.Journeys.FirstOrDefaultAsync(j => j.TicketReference == ticketGuid);

            if (journey == null)
                return NotFound(new { message = "Invalid Ticket: Not found in system." });

            // Uses strong Enum
            if (journey.Status != JourneyStatus.Active)
                return BadRequest(new { message = $"Ticket Rejected: Already {journey.Status}." });

            var shift = await _context.ConductorShifts.FirstOrDefaultAsync(s => s.Id == request.ShiftId);
            if (shift == null || shift.ShiftEndTime != null)
                return BadRequest(new { message = "Invalid or closed shift. Cannot process scan." });

            // Uses strong Enum
            journey.Status = JourneyStatus.Completed;
            journey.RedeemedAt = DateTime.UtcNow;

            // Increment the counter directly on the shift for the summary screen
            shift.DigitalTicketScanned += 1;

            _context.Journeys.Update(journey);
            _context.ConductorShifts.Update(shift);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ticket Validated", fareAmount = journey.TotalCost });
        }

        [HttpPost("cash-sale")]
        public async Task<IActionResult> RecordCashSale([FromBody] CashSaleRequest request)
        {
            var shift = await _context.ConductorShifts
        .Include(s => s.Route)
        .FirstOrDefaultAsync(s => s.Id == request.ShiftId);

            if (shift == null || shift.ShiftEndTime != null)
                return BadRequest(new { message = "Invalid or closed shift." });

            // 1. Find the Terminal (Last) Stop of this specific Route
            // Logic: Find a segment where its Destination is NEVER used as an Origin on this same route.
            var terminalSegment = await _context.RouteSegments
                .FirstOrDefaultAsync(rs => rs.RouteId == shift.RouteId &&
                    !_context.RouteSegments.Any(other => other.RouteId == shift.RouteId && other.OriginStopId == rs.DestinationStopId));

            if (terminalSegment == null)
                return BadRequest(new { message = "Route configuration error: Cannot determine terminal stop." });

            int endStopId = terminalSegment.DestinationStopId;

            // 2. Calculate Fares
            decimal fareTotal = shift.Route.BaseFare * request.PassengerCount;
            shift.TotalCashCollected += fareTotal;

            var issuedTickets = new List<PrintedTicketDto>();

            // 3. Issue the tickets
            for (int i = 0; i < request.PassengerCount; i++)
            {
                // Use the factory, passing in the dynamically found terminal stop
                var walkOnJourney = _journeyFactory.CreateCashJourney(shift.Id, shift.Route.BaseFare, endStopId);

                _context.Journeys.Add(walkOnJourney);

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

            // Save everything. The FK constraints are now perfectly satisfied!
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cash sale recorded successfully.",
                amountAdded = fareTotal,
                newTotal = shift.TotalCashCollected,
                receipts = issuedTickets
            });
        }

        [HttpGet("summary")]
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
    }
}
