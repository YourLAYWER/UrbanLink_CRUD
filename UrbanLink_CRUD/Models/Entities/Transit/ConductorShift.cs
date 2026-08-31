using Microsoft.Identity.Client;

namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class ConductorShift
    {
        public int Id { get; set; }

        public string ConductorId { get; set; } // Links to your Identity User
        public int BusId { get; set; }          // Which bus are they in?
        public Bus Bus {  get; set; }
        public int RouteId { get; set; }        // Which route are they driving?
        public TransitRoute Route { get; set; }

        public DateTime ShiftStartTime { get; set; }
        public DateTime? ShiftEndTime { get; set; } // Null until they clock out

        // Financial tracking for walk-on passengers
        public decimal TotalCashCollected { get; set; }

        public int DigitalTicketScanned { get; set; } = 0;

        // Add this at the bottom:
        public ICollection<IncidentReport> IncidentReports { get; set; }
    }
}
