using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Entities;
using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Controllers.TransitControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FleetController : ControllerBase
    {
        private readonly ULDBContext _context;

        public FleetController(ULDBContext context)
        {
            _context = context;
        }

        //[HttpGet("buses/available")]
        //[Authorize(Roles = "Conductor, Driver, Admin")]
        //public async Task<IActionResult> GetAvailableBuses()
        //{
        //    // A single trip to the database!
        //    var availableBuses = await _context.Buses
        //        .Where(b => b.Status == BusStatus.Active &&
        //                   !_context.ConductorShifts.Any(cs => cs.BusId == b.Id && cs.ShiftEndTime == null))
        //        .Select(b => new
        //        {
        //            id = b.Id,
        //            fleetNumber = b.FleetNumber,
        //            label = $"{b.FleetNumber} ({b.LicensePlate})"
        //        })
        //        .ToListAsync();

        //    return Ok(new { data = availableBuses });
        //}
        [HttpGet]
        public async Task<IActionResult> GetFleet()
        {
            // Fetches all rows (Active and InMaintenance) without any Where clause
            var buses = await _context.Buses.ToListAsync();
            return Ok(new { data = buses });
        }
        [HttpGet("allbusesavailable")]
        [Authorize(Roles = "Conductor, Driver, Admin")]
        public async Task<IActionResult> GetAvailabl()
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "Conductor";

            var activeShifts = await _context.ConductorShifts
                .Where(cs => cs.ShiftEndTime == null)
                .Include(cs => cs.Conductor)
                .ToListAsync();

            var allBuses = await _context.Buses.ToListAsync();

            var availableBuses = new List<object>();

            foreach (var bus in allBuses)
            {
                var busShifts = activeShifts.Where(cs => cs.BusId == bus.Id).ToList();

                bool hasDriver = busShifts.Any(cs => cs.Conductor?.Role == Role.Driver);
                bool hasConductor = busShifts.Any(cs => cs.Conductor?.Role == Role.Conductor);

                bool canSelect = true;

                if ("Driver".Equals(userRole, StringComparison.OrdinalIgnoreCase) && hasDriver)
                {
                    canSelect = false;
                }
                else if ("Conductor".Equals(userRole, StringComparison.OrdinalIgnoreCase) && hasConductor)
                {
                    canSelect = false;
                }

                availableBuses.Add(new
                {
                    id = bus.Id,
                    fleetNumber = bus.FleetNumber,
                    status = bus.Status.ToString(), // Will output "Active" or "InMaintenance"
                    canSelect = canSelect,
                    label = $"{bus.FleetNumber} ({bus.LicensePlate}) - Status: {bus.Status} [Driver: {(hasDriver ? "Filled" : "Open")} | Conductor: {(hasConductor ? "Filled" : "Open")}]"
                });
            }

            return Ok(new { data = availableBuses });
        }

        [HttpGet("buses/available")]
        [Authorize(Roles = "Conductor, Driver, Admin")]
        public async Task<IActionResult> GetAvailableBuses()
        {
            // 1. Get the role of the currently logged-in user from their token
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "Conductor";

            // 2. Fetch all active shifts with their associated User details to check roles
            var activeShifts = await _context.ConductorShifts
                .Where(cs => cs.ShiftEndTime == null)
                .Include(cs => cs.Conductor) // Ensure your ConductorShift has a navigation property to the User/Conductor entity
                .ToListAsync();

            var allBuses = await _context.Buses
                .Where(b => b.Status == BusStatus.Active)
                .ToListAsync();

            var availableBuses = new List<object>();

            foreach (var bus in allBuses)
            {
                var busShifts = activeShifts.Where(cs => cs.BusId == bus.Id).ToList();

                bool hasDriver = busShifts.Any(cs => cs.Conductor?.Role == Role.Driver);
                bool hasConductor = busShifts.Any(cs => cs.Conductor?.Role == Role.Conductor);

                bool canSelect = true;

                // If the user is a Driver, block if the bus already has a Driver
                if ("Driver".Equals(userRole, StringComparison.OrdinalIgnoreCase) && hasDriver)
                {
                    canSelect = false;
                }
                // If the user is a Conductor, block if the bus already has a Conductor
                else if ("Conductor".Equals(userRole, StringComparison.OrdinalIgnoreCase) && hasConductor)
                {
                    canSelect = false;
                }

                if (canSelect)
                {
                    availableBuses.Add(new
                    {
                        id = bus.Id,
                        fleetNumber = bus.FleetNumber,
                        label = $"{bus.FleetNumber} ({bus.LicensePlate}) [Driver: {(hasDriver ? "Filled" : "Open")} | Conductor: {(hasConductor ? "Filled" : "Open")}]"
                    });
                }
            }

            return Ok(new { data = availableBuses });
        }
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin, Driver, Conductor")]
        public async Task<IActionResult> UpdateBusStatus(int id, [FromBody] UpdateBusStatusRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new { message = "Request body or status cannot be empty." });
            }

            var bus = await _context.Buses.FindAsync(id);
            if (bus == null)
                return NotFound(new { message = "Bus not found." });

            // Handle both string names ("Active", "InMaintenance") and numeric values/enums safely
            if (Enum.TryParse<BusStatus>(request.Status.Trim(), true, out var parsedStatus))
            {
                bus.Status = parsedStatus;
            }
            else
            {
                // Fallback mapping if string doesn't match enum name directly
                if (request.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    bus.Status = (BusStatus)1; // Adjust based on your enum definition
                else if (request.Status.Equals("InMaintenance", StringComparison.OrdinalIgnoreCase) || request.Status.Equals("Maintenance", StringComparison.OrdinalIgnoreCase))
                    bus.Status = (BusStatus)2; // Adjust based on your enum definition
                else
                    return BadRequest(new { message = $"Invalid status value provided: '{request.Status}'" });
            }

            _context.Buses.Update(bus);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Bus status updated successfully.", busId = bus.Id, newStatus = bus.Status.ToString() });
        }

        public class UpdateBusStatusRequest
        {
            public string Status { get; set; } = string.Empty;
        }
    }
}
