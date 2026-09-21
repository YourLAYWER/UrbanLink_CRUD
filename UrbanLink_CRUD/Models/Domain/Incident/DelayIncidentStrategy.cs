using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Dtos.TransitDtos;
using UrbanLink_CRUD.Models.Entities.Transit;
using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Models.Domain.Incident
{
    public class DelayIncidentStrategy : IIncidentStrategy
    {
        private readonly ULDBContext _context;

        public DelayIncidentStrategy(ULDBContext context)
        {
            _context = context;
        }

        public int CategoryId => 5;

        public async Task ProcessIncidentAsync(IncidentReportRequestDto request, int conductorShiftId)
        {
            // 1. Create the SQL Server record
            var incident = new IncidentReport
            {
                ConductorShiftId = conductorShiftId,
                ReportedAt = DateTime.UtcNow,
                IsEmergency = request.IsEmergency,
                Category = IncidentCategory.Delay,
                Description = request.Description ?? "Traffic variance reported.",
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Status = IncidentStatus.Open
            };

            _context.IncidentReports.Add(incident);
            await _context.SaveChangesAsync();

            // 2. TODO: Call FirebaseService to push to "route_{routeId}_alerts" topic
            // (We will add a quick query here later to find the RouteId via the ConductorShiftId)
        }
    }
}
