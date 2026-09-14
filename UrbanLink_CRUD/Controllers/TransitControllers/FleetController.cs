using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;
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

        [HttpGet("buses/available")]
        [Authorize(Roles = "Conductor, Driver, Admin")]
        public async Task<IActionResult> GetAvailableBuses()
        {
            // A single trip to the database!
            var availableBuses = await _context.Buses
                .Where(b => b.Status == BusStatus.Active &&
                           !_context.ConductorShifts.Any(cs => cs.BusId == b.Id && cs.ShiftEndTime == null))
                .Select(b => new
                {
                    id = b.Id,
                    fleetNumber = b.FleetNumber,
                    label = $"{b.FleetNumber} ({b.LicensePlate})"
                })
                .ToListAsync();

            return Ok(new { data = availableBuses });
        }
    }
}
