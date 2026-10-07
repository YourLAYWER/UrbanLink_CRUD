using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Dtos.TransitDtos;

namespace UrbanLink_CRUD.Models.Domain.Incident
{
    public class MedicalIncidentStrategy : IIncidentStrategy
    {
        private readonly ULDBContext _context;

        public MedicalIncidentStrategy(ULDBContext context)
        {
            _context = context;
        }

        public int CategoryId => 6;

        public async Task ProcessIncidentAsync(IncidentReportRequestDto request, int routeId)
        {
            // 1. Log to an internal audit table instead of public alerts
            //var audit = new DepotAuditLog
            //{
            //    IncidentType = "Medical Emergency",
            //    Description = request.Details,
            //    GpsLocation = $"{request.Lat}, {request.Lng}",
            //    ReportedAt = DateTime.UtcNow
            //};

            //_context.DepotAuditLogs.Add(audit);
            //await _context.SaveChangesAsync();

            // 2. TODO: Trigger internal SignalR dashboard alert to Depot Security
        }
    }
}
