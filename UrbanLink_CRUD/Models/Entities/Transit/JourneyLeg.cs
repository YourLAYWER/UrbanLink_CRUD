namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class JourneyLeg
    {
        public int ID { get; set; }

        // Order this leg appears in the overall journey (1 = first leg, 2 = second leg, etc.)
        public int SequenceOrder { get; set; }

        // Foreign Keys
        public int JourneyID { get; set; }
        public int SegmentID { get; set; }

        // Navigation Properties
        public virtual Journey? Journey { get; set; }
        public virtual RouteSegment? RouteSegment { get; set; }
    }
}