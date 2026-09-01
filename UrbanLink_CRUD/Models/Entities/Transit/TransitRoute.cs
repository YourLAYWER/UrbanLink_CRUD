using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class TransitRoute
    {
        public int Id { get; set; }
        public string RouteName { get; set; } = string.Empty; // e.g., "Route 10"
        public TransitType TransportType { get; set; } // "Bus", "Train"
        public decimal BaseFare { get; set; }
        public int ProviderId { get; set; }

        // Navigation Properties
        public virtual TransitProvider? Provider { get; set; }
        public virtual ICollection<RouteSegment> Segments { get; set; } = new List<RouteSegment>();

        // NEW: One Route can have multiple schedules (e.g., Weekday, Weekend, Holiday)
        public virtual ICollection<RouteSchedule> Schedules { get; set; } = new List<RouteSchedule>();
    }
}
