using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Entities.Transit;

namespace UrbanLink_CRUD.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin")]  // uncomment once role checks are finalised across the team
    public class ShiftController : ControllerBase
    {
        private readonly ULDBContext _context;

        public ShiftController(ULDBContext context)
        {
            _context = context;
        }

        // GET: api/admin/shift/active
        // Live dashboard: every currently active shift, with driver/vehicle/route detail
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<object>>> GetActiveShifts()
        {
            var shifts = await _context.Shifts
                .Include(s => s.Driver)
                .Include(s => s.Vehicle)
                .Where(s => s.Status == "Active")
                .Select(s => new
                {
                    s.Id,
                    DriverName = s.Driver != null ? s.Driver.Name : "Unknown",
                    VehicleRegistration = s.Vehicle != null ? s.Vehicle.RegistrationNumber : "Unassigned",
                    Route = s.RouteSegmentId.HasValue ? $"Segment #{s.RouteSegmentId.Value}" : "Not set",
                    s.StartTime,
                    s.Status
                })
                .ToListAsync();

            return Ok(shifts);
        }

        // POST: api/admin/shift/5/force-close
        // Override control: force-close a stale or abandoned shift
        [HttpPost("{id}/force-close")]
        public async Task<IActionResult> ForceCloseShift(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null) return NotFound(new { message = $"Shift with ID {id} not found." });

            if (shift.Status != "Active")
                return BadRequest(new { message = "Only an active shift can be force-closed." });

            shift.Status = "ForceClosed";
            shift.EndTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Shift {id} was force-closed.", shift });
        }
    }
}