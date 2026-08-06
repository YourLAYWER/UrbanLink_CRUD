namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class TransitRoute
    {
        public int Id { get; set; }
        public string RouteName { get; set; } = string.Empty; // e.g., "Route 10"
        public string TransportType { get; set; } = string.Empty; // "Bus", "Train"
        public decimal BaseFare { get; set; }
        public int ProviderId { get; set; }

        // Navigation Properties
        public virtual TransitProvider? Provider { get; set; }
        public virtual ICollection<RouteSegment> Segments { get; set; } = new List<RouteSegment>();
    }
}
