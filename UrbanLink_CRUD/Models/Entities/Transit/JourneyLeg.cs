using System.Text.Json.Serialization;
using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class JourneyLeg
    {
        public int Id { get; set; }
        public int JourneyId { get; set; }
        public int SequenceOrder { get; set; }
        public int RouteId { get; set; }
        public int BoardingStopId { get; set; }
        public int DestinationStopId { get; set; }
        public decimal LegFare { get; set; }

        // Extracted from your algorithm
        public int LegDurationMinutes { get; set; }
        public Guid TicketReference { get; set; } = Guid.NewGuid();
        public IncidentStatus Status { get; set; } = IncidentStatus.Open;
        public DateTime? RedeemedAt { get; set; }

        public virtual Journey? Journey { get; set; }
        public virtual TransitRoute? Route { get; set; }
        public virtual Stop? BoardingStop { get; set; }
        public virtual Stop? DestinationStop { get; set; }
    }
}