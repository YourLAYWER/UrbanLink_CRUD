using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Dtos.Transit;
using UrbanLink_CRUD.Models.Entities.Transit;

namespace UrbanLink_CRUD.Controllers.TransitControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RouteSchedulesController : ControllerBase
    {
        private readonly ULDBContext _context;

        public RouteSchedulesController(ULDBContext context)
        {
            _context = context;
        }

        private static bool TryParseTime(string value, out TimeSpan time)
        {
            return TimeSpan.TryParseExact(value, @"hh\:mm", CultureInfo.InvariantCulture, out time);
        }

        // GET: api/RouteSchedules/route/5
        [HttpGet("route/{routeId:int}")]
        public async Task<ActionResult<IEnumerable<object>>> GetSchedulesForRoute(int routeId)
        {
            var schedules = await _context.RouteSchedules
                .Where(s => s.RouteId == routeId)
                .AsNoTracking()
                .ToListAsync();

            return schedules.Select(s => new
            {
                s.ScheduleId,
                s.RouteId,
                s.DaysOfOperation,
                FirstDepartureTime = s.FirstDepartureTime.ToString(@"hh\:mm"),
                LastDepartureTime = s.LastDepartureTime.ToString(@"hh\:mm"),
                s.FrequencyMinutes
            }).ToList();
        }

        // POST: api/RouteSchedules
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleRequestDto request)
        {
            if (!TryParseTime(request.FirstDepartureTime, out var first))
                return BadRequest(new { message = "FirstDepartureTime must be in HH:mm format." });
            if (!TryParseTime(request.LastDepartureTime, out var last))
                return BadRequest(new { message = "LastDepartureTime must be in HH:mm format." });
            if (request.FrequencyMinutes <= 0)
                return BadRequest(new { message = "FrequencyMinutes must be greater than zero." });

            var routeExists = await _context.TransitRoutes.AnyAsync(r => r.Id == request.RouteId);
            if (!routeExists) return BadRequest(new { message = $"Route {request.RouteId} does not exist." });

            var schedule = new RouteSchedule
            {
                RouteId = request.RouteId,
                DaysOfOperation = request.DaysOfOperation,
                FirstDepartureTime = first,
                LastDepartureTime = last,
                FrequencyMinutes = request.FrequencyMinutes
            };

            _context.RouteSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            return Ok(new { schedule.ScheduleId });
        }

        // PUT: api/RouteSchedules/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] UpdateScheduleRequestDto request)
        {
            var schedule = await _context.RouteSchedules.FindAsync(id);
            if (schedule == null) return NotFound();

            if (!TryParseTime(request.FirstDepartureTime, out var first))
                return BadRequest(new { message = "FirstDepartureTime must be in HH:mm format." });
            if (!TryParseTime(request.LastDepartureTime, out var last))
                return BadRequest(new { message = "LastDepartureTime must be in HH:mm format." });
            if (request.FrequencyMinutes <= 0)
                return BadRequest(new { message = "FrequencyMinutes must be greater than zero." });

            schedule.DaysOfOperation = request.DaysOfOperation;
            schedule.FirstDepartureTime = first;
            schedule.LastDepartureTime = last;
            schedule.FrequencyMinutes = request.FrequencyMinutes;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/RouteSchedules/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await _context.RouteSchedules.FindAsync(id);
            if (schedule == null) return NotFound();

            _context.RouteSchedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
