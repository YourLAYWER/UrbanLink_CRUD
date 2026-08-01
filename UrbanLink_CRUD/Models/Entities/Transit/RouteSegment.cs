namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class RouteSegment
    {
        public int ID { get; set; }

        // Foreign Key to TransitProvider
        public int ProviderID { get; set; }

        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;

        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal BaseFare { get; set; }

        // Navigation Property: Each route segment belongs to a transit provider
        public virtual TransitProvider? TransitProvider { get; set; }
    }
}
