using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Domain.Incident;
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
        private readonly IncidentStrategyResolver _strategyResolver;
        public IncidentController(ULDBContext context, IncidentStrategyResolver strategyResolver)
        {
            _context = context;
            _strategyResolver = strategyResolver;
        }

        [HttpPost("report")]
        [Authorize] // Ensure the tablet is authenticated
        public async Task<IActionResult> LogIncident([FromBody] IncidentReportRequestDto request)
        {
            // 1. Extract the authenticated conductor's/driver's ID from the JWT token
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int conductorId))
                return Unauthorized(new { message = "Invalid token or user ID." });

            // 2. Automatically find the active shift for this operator
            var shift = await _context.ConductorShifts
                .FirstOrDefaultAsync(s => s.ConductorId == conductorId && s.ShiftEndTime == null);

            if (shift == null)
                return BadRequest(new { message = "No active shift found. Cannot log incident." });

            try
            {
                // 3. Resolve the specific strategy based on the incoming category ID
                // Cast to int if request.Category is an enum in your DTO
                var strategy = _strategyResolver.Resolve((int)request.Category);

                // 4. Execute the strategy (Saves to DB and fires Firebase notification if applicable)
                await strategy.ProcessIncidentAsync(request, shift.Id);

                return Ok(new
                {
                    message = request.IsEmergency ? "EMERGENCY SOS LOGGED." : "Routine incident logged successfully.",
                    timestamp = DateTime.Now
                });
            }
            catch (NotSupportedException ex)
            {
                // Catches the exception thrown if the driver selects a category we haven't built a strategy for yet
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                // Generic fallback for database or Firebase failures
                return StatusCode(500, new { error = "An error occurred while processing the incident.", details = ex.Message });
            }
        }

        [HttpGet("routes/{routeId}/status")]
        public async Task<IActionResult> GetRouteStatus(int routeId)
        {
            // 1. Define the expiration window (e.g., ignore anything older than __ hours)
            var expirationWindow = DateTime.Now.AddHours(-1);

            var activeAlert = await _context.IncidentReports
                .Include(i => i.ConductorShift)
                .Where(i => i.ConductorShift.RouteId == routeId
                         && i.Status == IncidentStatus.Open
                         && i.Category == IncidentCategory.Delay

                         // 2. THE FAIL-SAFE: Only fetch if it happened recently
                         && i.ReportedAt >= expirationWindow

                       // 3. OPTIONAL: If your ConductorShift model tracks Active status, add this:
                       //&& i.ConductorShift.IsActive == true
                       )
                .OrderByDescending(i => i.ReportedAt)
                .Select(i => new
                {
                    message = i.Description,
                    reportedAt = i.ReportedAt,
                    isEmergency = i.IsEmergency
                })
                .FirstOrDefaultAsync();

            if (activeAlert != null)
            {
                return Ok(new { hasDelay = true, alert = activeAlert });
            }

            return Ok(new { hasDelay = false });
        }

        // GET: api/admin/incident
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetIncidents()
        {
            var incidents = await _context.IncidentReports
                .Include(i => i.ConductorShift)
                .ThenInclude(cs => cs.Conductor)
                .OrderByDescending(i => i.ReportedAt)
                .Select(i => new
                {
                    i.Id,
                    ReportedByName = i.ConductorShift != null && i.ConductorShift.Conductor != null
                                     ? i.ConductorShift.Conductor.Name
                                     : "Unknown",
                    i.ConductorShiftId,
                    i.ReportedAt,
                    i.IsEmergency,
                    Category = i.Category.ToString(),
                    i.Description,
                    Status = i.Status.ToString(),
                    i.Latitude,
                    i.Longitude,
                    i.ResolvedAt
                })
                .ToListAsync();

            return Ok(incidents);
        }

        public class UpdateIncidentDto
        {
            public string Status { get; set; } = string.Empty; // "Open", "Investigating", "Resolved"
        }

        // PUT: api/admin/incident/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIncident(int id, UpdateIncidentDto dto)
        {
            var incident = await _context.IncidentReports.FindAsync(id);
            if (incident == null) return NotFound(new { message = $"Incident with ID {id} not found." });

            var validStatuses = new[] { "Open", "Investigating", "Resolved" };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest(new { message = $"Status must be one of: {string.Join(", ", validStatuses)}" });

            if (Enum.TryParse(typeof(IncidentStatus), dto.Status, out var parsedStatus))
            {
                incident.Status = (IncidentStatus)parsedStatus;
            }

            // Automatically set ResolvedAt timestamp when an incident is closed
            if (dto.Status == "Resolved" && incident.ResolvedAt == null)
            {
                incident.ResolvedAt = DateTime.Now; // Or DateTime.Now depending on your timezone logic
            }

            await _context.SaveChangesAsync();
            return Ok(incident);
        }

    }
}
