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
            // 1. Parse the scanned QR Code string into a Guid (since TicketReference is a Guid)
            if (!Guid.TryParse(request.QRCodeData, out Guid ticketGuid))
            {
                return BadRequest(new { message = "Invalid QR Code format." });
            }

            // 2. Find the Journey in the database
            var journey = await _context.Journeys.FirstOrDefaultAsync(j => j.TicketReference == ticketGuid);

            if (journey == null)
            {
                return NotFound(new { message = "Invalid Ticket: Not found in system." });
            }

            // 3. Check if the journey is already used
            if (journey.Status != "Active")
            {
                return BadRequest(new { message = $"Ticket Rejected: Already {journey.Status}." });
            }

            // 4. Verify the shift is valid so we can link the audit trail
            var shift = await _context.ConductorShifts.FirstOrDefaultAsync(s => s.Id == request.ShiftId);
            if (shift == null || shift.ShiftEndTime != null)
            {
                return BadRequest(new { message = "Invalid or closed shift. Cannot process scan." });
            }

            // 5. Mark the ticket as used and lock in the Audit Trail
            journey.Status = "Used";
            journey.RedeemedAt = DateTime.UtcNow;

            // THIS line is what makes your dynamic ticket counter work on the summary screen!
            journey.IssuedDuringShiftId = shift.Id;

            _context.Journeys.Update(journey);
            await _context.SaveChangesAsync();

            // 6. Return success and the actual fare amount paid for this journey
            return Ok(new
            {
                message = "Ticket Validated",
                fareAmount = journey.TotalCost
            });
        }

        //[HttpPost("cash-sale")]
        //public async Task<IActionResult> RecordCashSale([FromBody] CashSaleRequest request)
        //{
        //    var shift = await _context.ConductorShifts
        //        .Include(s => s.Route)
        //        .FirstOrDefaultAsync(s => s.Id == request.ShiftId);

        //    if (shift == null || shift.ShiftEndTime != null)
        //    {
        //        return BadRequest(new { message = "Invalid or closed shift." });
        //    }

        //    decimal fareTotal = shift.Route.BaseFare * request.PassengerCount;
        //    shift.TotalCashCollected += fareTotal;

        //    // NEW: Generate the physical tickets for the slip machine
        //    var issuedTickets = new List<PrintedTicketDto>();

        //    for (int i = 0; i < request.PassengerCount; i++)
        //    {
        //        // 1. Create a unique ID for the printed slip
        //        string uniqueTicketId = $"CASH-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        //        // 2. Save it to your database so inspectors can verify it later
        //        var newTicket = new Ticket
        //        {
        //            QRCodeData = uniqueTicketId,
        //            Status = Models.Enums.TicketStatus.Used, // Marked as 'Used' instantly since they are boarding right now
        //                                                     // Optional: You could link it to the ShiftId here if your Ticket model supports it
        //        };
        //        _context.Tickets.Add(newTicket);

        //        // 3. Add the details to the response for the Bluetooth printer
        //        issuedTickets.Add(new PrintedTicketDto
        //        {
        //            TicketId = uniqueTicketId,
        //            RouteName = shift.Route.RouteName,
        //            FarePaid = shift.Route.BaseFare,
        //            IssuedAt = DateTime.UtcNow
        //        });
        //    }

        //    _context.ConductorShifts.Update(shift);
        //    await _context.SaveChangesAsync(); // Saves the cash total AND the new tickets

        //    // Return everything to Android
        //    return Ok(new
        //    {
        //        message = "Cash sale recorded successfully.",
        //        amountAdded = fareTotal,
        //        newTotal = shift.TotalCashCollected,
        //        receipts = issuedTickets // The printer data!
        //    });
        //}

        [HttpPost("cash-sale")]
        public async Task<IActionResult> RecordCashSale([FromBody] CashSaleRequest request)
        {
            var shift = await _context.ConductorShifts
                .Include(s => s.Route)
                .FirstOrDefaultAsync(s => s.Id == request.ShiftId);

            if (shift == null || shift.ShiftEndTime != null)
            {
                return BadRequest(new { message = "Invalid or closed shift." });
            }

            // Calculate total fare
            decimal fareTotal = shift.Route.BaseFare * request.PassengerCount;
            shift.TotalCashCollected += fareTotal;

            var issuedTickets = new List<PrintedTicketDto>();

            for (int i = 0; i < request.PassengerCount; i++)
            {
                // 1. Generate the Guid for the database
                Guid newTicketGuid = Guid.NewGuid();

                // 2. Create a short, readable version for the printed slip
                string displayTicketId = $"CASH-{newTicketGuid.ToString().Substring(0, 8).ToUpper()}";

                // 3. Create the anonymous Journey mapping to your EXACT model properties
                var walkOnJourney = new Journey
                {
                    TicketReference = newTicketGuid,   // Maps to your Guid property
                    Status = "Used",                   // Your model uses a string for Status
                    TotalCost = shift.Route.BaseFare,  // Maps to your TotalCost property
                    UserId = null,                     // Leaves it anonymous
                    IssuedDuringShiftId = shift.Id,    // Locks in the audit trail!
                    RedeemedAt = DateTime.UtcNow,      // Marks it used immediately
                    TotalDurationMinutes = 0           // Default for cash walk-ons
                };

                _context.Journeys.Add(walkOnJourney);

                // 4. Send the data to the Bluetooth printer
                issuedTickets.Add(new PrintedTicketDto
                {
                    TicketId = displayTicketId,
                    RouteName = shift.Route.RouteName,
                    FarePaid = shift.Route.BaseFare,
                    IssuedAt = walkOnJourney.RedeemedAt.Value
                });
            }

            _context.ConductorShifts.Update(shift);

            // Saves the updated cash total AND the new anonymous Journeys
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cash sale recorded successfully.",
                amountAdded = fareTotal,
                newTotal = shift.TotalCashCollected,
                receipts = issuedTickets
            });
        }

        // Get Shift Summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetShiftSummary()
        {
            var conductorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Find the active shift
            var activeShift = await _context.ConductorShifts
                .Include(cs => cs.Bus)
                .Include(cs => cs.Route)
                .FirstOrDefaultAsync(cs => cs.ConductorId == conductorId && cs.ShiftEndTime == null);

            if (activeShift == null)
                return BadRequest(new { message = "No active shift found." });

            // Calculate live ticket count
            int digitalScanCount = await _context.Journeys
                .CountAsync(j => j.IssuedDuringShiftId == activeShift.Id && j.UserId != null);

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
                    digitalTicketsScanned = digitalScanCount
                }
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

            // 2. Find the conductor's currently active shift
            var activeShift = await _context.ConductorShifts
                .Include(cs => cs.Bus)
                .Include(cs => cs.Route)
                .FirstOrDefaultAsync(cs => cs.ConductorId == conductorId && cs.ShiftEndTime == null);

            if (activeShift == null)
            {
                return BadRequest(new { message = "No active shift found. You are already clocked out." });
            }

            // 3. Clock them out
            activeShift.ShiftEndTime = DateTime.UtcNow;

            // 4. GET THE ACTUAL TICKET COUNT FROM JOURNEYS
            // We count how many Journeys are linked to this shift WHERE the user had an app account (UserId != null).
            int digitalScanCount = await _context.Journeys
                .CountAsync(j => j.IssuedDuringShiftId == activeShift.Id && j.UserId != null);

            // (Optional: You could also count cash walk-ons with j.UserId == null if you wanted to display that!)

            // 5. Save changes to the database
            _context.ConductorShifts.Update(activeShift);
            await _context.SaveChangesAsync();

            // 6. Return the final summary data
            return Ok(new
            {
                message = "Shift ended successfully.",
                shiftSummary = new
                {
                    shiftId = activeShift.Id,
                    busFleetNumber = activeShift.Bus.FleetNumber, // Null safety added
                    routeName = activeShift.Route.RouteName,      // Null safety added
                    startTime = activeShift.ShiftStartTime,
                    endTime = activeShift.ShiftEndTime,
                    totalCashHandover = activeShift.TotalCashCollected,
                    digitalTicketsScanned = digitalScanCount // Dynamic count injected here!
                }
            });
        }
    }
}
