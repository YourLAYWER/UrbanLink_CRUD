namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class RouteSegment
    {
        public int Id { get; set; }

        // Foreign Keys to the Stop table
        public int OriginStopId { get; set; }
        public int DestinationStopId { get; set; }

        // Foreign Key to the Route table
        public int RouteId { get; set; }

        public int TravelTimeMinutes { get; set; }

        // Navigation Properties
        public virtual Stop? OriginStop { get; set; }
        public virtual Stop? DestinationStop { get; set; }
        public virtual TransitRoute? Route { get; set; }
    }
}
