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
    }
}
