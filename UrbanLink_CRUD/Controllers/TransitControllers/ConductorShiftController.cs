using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Dtos.TransitDtos;
using UrbanLink_CRUD.Models.Entities.Transit;

namespace UrbanLink_CRUD.Controllers.TransitControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConductorShiftController : ControllerBase
    {
        private readonly ULDBContext _context;

        public ConductorShiftController(ULDBContext context)
        {
            _context = context;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartShift([FromBody] StartShiftRequest request)
        {
            // 1. Get the Conductor's ID securely from their login token
            var conductorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(conductorId))
            {
                return Unauthorized(new { message = "Invalid user token." });
            }

            // 2. Prevent starting a new shift if they are already on one
            var activeShift = await _context.ConductorShifts
                .FirstOrDefaultAsync(cs => cs.ConductorId == conductorId && cs.ShiftEndTime == null);

            if (activeShift != null)
            {
                return BadRequest(new
                {
                    message = "You already have an active shift.",
                    shiftId = activeShift.Id
                });
            }

            // 3. Verify the Bus and Route actually exist
            var busExists = await _context.Buses.AnyAsync(b => b.Id == request.BusId);
            var routeExists = await _context.TransitRoutes.AnyAsync(r => r.Id == request.RouteId);

            if (!busExists || !routeExists)
            {
                return NotFound(new { message = "The selected Bus or Route does not exist." });
            }

            // 4. Create and save the new shift
            var newShift = new ConductorShift
            {
                ConductorId = conductorId,
                BusId = request.BusId,
                RouteId = request.RouteId,
                ShiftStartTime = DateTime.UtcNow, // Always store UTC time in the database
                TotalCashCollected = 0
            };

            _context.ConductorShifts.Add(newShift);
            await _context.SaveChangesAsync();

            // 5. Return the newly generated Shift ID to the Android app
            return Ok(new
            {
                message = "Shift started successfully.",
                shiftId = newShift.Id,
                startTime = newShift.ShiftStartTime
            });
        }


        [HttpPost("scan")]
        public async Task<IActionResult> ScanTicket([FromBody] ScanTicketRequest request)
        {
            // 1. Find the ticket in the database
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.QRCodeData == request.QRCodeData);

            if (ticket == null)
            {
                return NotFound(new { message = "Invalid Ticket: Not found in system." });
            }

            // 2. Check if the ticket is already used
            if (ticket.Status != Models.Enums.TicketStatus.Active)
            {
                return BadRequest(new { message = $"Ticket Rejected: Already {ticket.Status}." });
            }

            // 3. Mark the ticket as used
            ticket.Status = Models.Enums.TicketStatus.Used; // Ensure you have 'Used' in your TicketStatus Enum!
            _context.Tickets.Update(ticket);

            // (Optional: You could create a 'TicketScanLog' record here linking the ticket to the ShiftId)

            await _context.SaveChangesAsync();

            // 4. Return success and the base fare amount (hardcoded to 15.00 for this example, but you'd pull it from the Ticket/Route)
            return Ok(new
            {
                message = "Ticket Validated",
                fareAmount = 15.00m
            });
        }


        [HttpPost("end")]
        public async Task<IActionResult> EndShift()
        {
            // 1. Get the Conductor's ID securely from their login token
            var conductorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(conductorId))
            {
                return Unauthorized(new { message = "Invalid user token." });
            }

            // 2. Find the conductor's currently active shift (ShiftEndTime is null)
            var activeShift = await _context.ConductorShifts
                .Include(cs => cs.Bus) // Optional: Bring in Bus data for the summary screen
                .Include(cs => cs.Route) // Optional: Bring in Route data for the summary
                .FirstOrDefaultAsync(cs => cs.ConductorId == conductorId && cs.ShiftEndTime == null);

            if (activeShift == null)
            {
                return BadRequest(new { message = "No active shift found. You are already clocked out." });
            }

            // 3. Clock them out
            activeShift.ShiftEndTime = DateTime.UtcNow;

            // Note: If you add a TicketScans table later, you would count them here:
            // var scanCount = await _context.TicketScans.CountAsync(t => t.ShiftId == activeShift.Id);

            // 4. Save changes to the database
            _context.ConductorShifts.Update(activeShift);
            await _context.SaveChangesAsync();

            // 5. Return the final summary data so the Android app can display the Shift Summary screen
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
                    totalCashHandover = activeShift.TotalCashCollected // This was initialized to 0 in StartShift
                                                                       // digitalTicketsScanned = scanCount // Pass this back once we build the scanning logic!
                }
            });
        }
    }
}
