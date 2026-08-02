namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class RouteStop
    {
        public int ID { get; set; }

        // Foreign Keys
        public int SegmentID { get; set; }
        public int StopID { get; set; }

        // Order this stop appears in along the route (1 = first, 2 = second, etc.)
        public int StopSequence { get; set; }

        // Navigation Properties
        public virtual RouteSegment? RouteSegment { get; set; }
        public virtual Stop? Stop { get; set; }
    }
}