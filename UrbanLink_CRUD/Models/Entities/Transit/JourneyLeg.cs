using System.Text.Json.Serialization;

namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class JourneyLeg
    {
        public int Id { get; set; }
        public int JourneyId { get; set; }
        public int SequenceOrder { get; set; }

        // CHANGED: We drop the Provider/Type strings because RouteId gives us that data
        public int RouteId { get; set; }

        // CHANGED: From strings to Stop IDs
        public int BoardingStopId { get; set; }
        public int DestinationStopId { get; set; }

        public decimal LegFare { get; set; }

        // NEW: Extracted from your algorithm
        public int LegDurationMinutes { get; set; }

        public Guid TicketReference { get; set; } = Guid.NewGuid();
        public string Status { get; set; } = "Active";
        public DateTime? RedeemedAt { get; set; }

        public virtual Journey? Journey { get; set; }
        public virtual TransitRoute? Route { get; set; }
        public virtual Stop? BoardingStop { get; set; }
        public virtual Stop? DestinationStop { get; set; }
    }
}