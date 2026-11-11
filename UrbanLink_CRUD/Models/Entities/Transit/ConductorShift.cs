using Microsoft.EntityFrameworkCore;

namespace UrbanLink_CRUD.Models.Entities.Transit
{
    [Index(nameof(ConductorId), nameof(ShiftEndTime), Name = "IX_ConductorShifts_ConductorId_ShiftEndTime")]
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

        public int DigitalTicketScanned { get; set; } = 0;

        // Links this shift directly to all incident reports filed by the conductor during their shift
        public virtual ICollection<IncidentReport> IncidentReports { get; set; } = new List<IncidentReport>();

        // Links this shift directly to all anonymous cash walk-ons
        public virtual ICollection<CashJourney> CashJourneys { get; set; } = new List<CashJourney>();

        // Links this shift directly to all card top-ups performed by the conductor
        public virtual ICollection<CardTopUp> CardTopUps { get; set; } = new List<CardTopUp>();
    }
}
