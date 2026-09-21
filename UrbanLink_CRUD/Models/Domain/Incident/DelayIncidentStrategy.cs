using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Domain.Incident.FireBase;
using UrbanLink_CRUD.Models.Dtos.TransitDtos;
using UrbanLink_CRUD.Models.Entities.Transit;
using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Models.Domain.Incident
{
    public class DelayIncidentStrategy : IIncidentStrategy
    {
        private readonly ULDBContext _context;
        private readonly IFirebaseNotificationService _firebaseService;

        public DelayIncidentStrategy(ULDBContext context, IFirebaseNotificationService firebaseService)
        {
            _context = context;
            _firebaseService = firebaseService;
        }

        public int CategoryId => 5;

        public async Task ProcessIncidentAsync(IncidentReportRequestDto request, int activeShiftId)
        {
            // 1. Create the SQL Server record
            var incident = new IncidentReport
            {
                ConductorShiftId = activeShiftId,
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
            // 2. Look up the RouteId tied to this shift so we know which FCM Topic to target
            var shift = await _context.ConductorShifts.FindAsync(activeShiftId);

            if (shift != null)
            {
                // 3. Trigger the Firebase Nudge
                await _firebaseService.SendRouteAlertAsync(
                    routeId: shift.RouteId,
                    title: "Transit Delay Alert",
                    message: incident.Description
                );
            }
        }
    }
}
