using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class TransitProvider
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TransitType Type { get; set; }// e.g., "Bus", "Train", "Tram"
        public string APIEndpoint { get; set; } = string.Empty; // Endpoint for fetching transit data


        // Navigation Property: A provider has many route segments
        public virtual ICollection<RouteSegment> RouteSegments { get; set; } = new List<RouteSegment>();
    }
}
