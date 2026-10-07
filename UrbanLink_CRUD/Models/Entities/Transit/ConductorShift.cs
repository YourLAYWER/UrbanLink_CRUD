using Microsoft.Identity.Client;

namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class ConductorShift
    {
        public int Id { get; set; }

        public int ConductorId { get; set; }
        public virtual User Conductor { get; set; }
        public int BusId { get; set; }
        public virtual Bus Bus { get; set; }

        public int RouteId { get; set; }
        public virtual TransitRoute Route { get; set; }

        public DateTime ShiftStartTime { get; set; }
        public DateTime? ShiftEndTime { get; set; }

        public decimal TotalCashCollected { get; set; }

        // Retained based on your structure, though dynamic counting is still possible via the Journeys table
        public int DigitalTicketScanned { get; set; } = 0;

        public virtual ICollection<IncidentReport> IncidentReports { get; set; } = new List<IncidentReport>();

        // THE AUDIT TRAIL: Links this shift directly to all anonymous cash walk-ons
        public virtual ICollection<CashJourney> CashJourneys { get; set; } = new List<CashJourney>();
    }
}
