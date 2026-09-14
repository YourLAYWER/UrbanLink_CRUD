using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Domain.Journey;
using UrbanLink_CRUD.Models.Dtos.TransitDtos;
using UrbanLink_CRUD.Models.Entities.Transit;
using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Controllers.TransitControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentController : ControllerBase
    {
        private readonly ULDBContext _context;
        public IncidentController(ULDBContext context)
        {
            _context = context;
        }

        [HttpPost("report")]
        public async Task<IActionResult> LogIncident([FromBody] IncidentReportRequestDto request)
        {
            // 1. Extract the authenticated conductor's ID from the JWT token
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int conductorId))
                return Unauthorized(new { message = "Invalid token or user ID." });

            // 2. Automatically find the active shift for this operator
            var shift = await _context.ConductorShifts
                .FirstOrDefaultAsync(s => s.ConductorId == conductorId && s.ShiftEndTime == null);

            if (shift == null)
                return BadRequest(new { message = "No active shift found. Cannot log incident." });

            // 3. Construct and save the incident record using your entity structure
            var incident = new IncidentReport
            {
                ConductorShiftId = shift.Id,
                IsEmergency = request.IsEmergency,
                Category = request.Category,
                Description = request.Description ?? string.Empty, // Prevents null reference if left blank
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                ReportedAt = DateTime.UtcNow,
                Status = IncidentStatus.Open // Assuming 'Open' is the starting state in your enum
            };

            _context.IncidentReports.Add(incident);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = request.IsEmergency ? "EMERGENCY SOS LOGGED." : "Routine incident logged successfully.",
                incidentId = incident.Id,
                timestamp = incident.ReportedAt
            });
        }
    }
}
