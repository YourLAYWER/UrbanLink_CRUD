using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;

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

        [HttpGet("buses/available")]
        public async Task<IActionResult> GetAvailableBuses()
        {
            // 1. Find the IDs of all buses currently being driven (ShiftEndTime is null)
            var busesOnActiveShifts = await _context.ConductorShifts
                .Where(cs => cs.ShiftEndTime == null)
                .Select(cs => cs.BusId)
                .ToListAsync();

            // 2. Fetch buses that are structurally sound AND parked at the depot
            var availableBuses = await _context.Buses
                .Where(b => b.Status == "Active" && !busesOnActiveShifts.Contains(b.Id))
                .Select(b => new
                {
                    id = b.Id,
                    fleetNumber = b.FleetNumber,
                    label = $"{b.FleetNumber} ({b.LicensePlate})" // Formatted nicely for the Android UI
                })
                .ToListAsync();

            return Ok(new { data = availableBuses });
        }
    }
}
