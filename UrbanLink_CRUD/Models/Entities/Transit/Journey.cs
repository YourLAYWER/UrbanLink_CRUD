using Microsoft.Extensions.Configuration.UserSecrets;

namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class Journey
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        // CHANGED: Now points to the physical Stop IDs
        public int StartStopId { get; set; }
        public int EndStopId { get; set; }

        public decimal TotalCost { get; set; }

        // NEW: Extracted from your algorithm's RoutingResult
        public int TotalDurationMinutes { get; set; }

        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid TicketReference { get; set; } = Guid.NewGuid();
        public DateTime? RedeemedAt { get; set; }

        public virtual ICollection<JourneyLeg> Legs { get; set; } = new List<JourneyLeg>();
        public virtual User? User { get; set; }
        public virtual Stop? StartStop { get; set; }
        public virtual Stop? EndStop { get; set; }
    }
}
