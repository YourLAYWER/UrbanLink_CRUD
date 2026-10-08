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
            var shifts = await _context.ConductorShifts
                .Include(s => s.Conductor)
                .Include(s => s.Bus)
                .Include(s => s.Route)
                .Where(s => s.ShiftEndTime == null) // In the new model, a null EndTime means "Active"
                .Select(s => new
                {
                    Id = s.Id,
                    // Map to the frontend's expected properties using the new relationships
                    DriverName = s.Conductor != null ? s.Conductor.Name : "Unknown",
                    VehicleRegistration = s.Bus != null ? s.Bus.LicensePlate : "Unassigned",
                    Route = s.Route != null ? s.Route.RouteName : "Not set",
                    StartTime = s.ShiftStartTime,
                    Status = "Active"
                })
                .ToListAsync();

            return Ok(shifts);
        }

        // POST: api/admin/shift/5/force-close
        // Override control: force-close a stale or abandoned shift
        [HttpPost("{id}/force-close")]
        public async Task<IActionResult> ForceCloseShift(int id)
        {
            var shift = await _context.ConductorShifts.FindAsync(id);
            if (shift == null) return NotFound(new { message = $"Shift with ID {id} not found." });

            // In the new model, if EndTime has a value, the shift is already closed
            if (shift.ShiftEndTime != null)
                return BadRequest(new { message = "Only an active shift can be force-closed." });

            // Force close it by setting the end time
            shift.ShiftEndTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Shift {id} was force-closed.", shift });
        }
    }
}