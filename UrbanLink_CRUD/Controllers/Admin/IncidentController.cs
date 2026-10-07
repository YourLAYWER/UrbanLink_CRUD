using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Entities.Transit;

namespace UrbanLink_CRUD.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin")]
    public class IncidentController : ControllerBase
    {
        private readonly ULDBContext _context;

        public IncidentController(ULDBContext context)
        {
            _context = context;
        }

        // GET: api/admin/incident
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetIncidents()
        {
            var incidents = await _context.Incidents
                .Include(i => i.ReportedBy)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new
                {
                    i.Id,
                    ReportedByName = i.ReportedBy != null ? i.ReportedBy.Name : "Unknown",
                    i.Type,
                    i.Description,
                    i.Status,
                    i.CreatedAt,
                    i.AdminNotes
                })
                .ToListAsync();

            return Ok(incidents);
        }

        public class UpdateIncidentDto
        {
            public string Status { get; set; } = string.Empty; // "Open", "Investigating", "Resolved"
            public string? AdminNotes { get; set; }
        }

        // PUT: api/admin/incident/5
        // Change status and/or add admin notes — no full CRUD here, just triage
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIncident(int id, UpdateIncidentDto dto)
        {
            var incident = await _context.Incidents.FindAsync(id);
            if (incident == null) return NotFound(new { message = $"Incident with ID {id} not found." });

            var validStatuses = new[] { "Open", "Investigating", "Resolved" };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest(new { message = $"Status must be one of: {string.Join(", ", validStatuses)}" });

            incident.Status = dto.Status;
            if (dto.AdminNotes != null) incident.AdminNotes = dto.AdminNotes;

            await _context.SaveChangesAsync();
            return Ok(incident);
        }
    }
}