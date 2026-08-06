using Microsoft.EntityFrameworkCore;

namespace UrbanLink_CRUD.Models.Entities.Transit
{
    [PrimaryKey(nameof(SegmentId), nameof(StopId))]
    public class RouteStop
    {
        // Composite primary key — SegmentId + StopId together uniquely identify a row.
        // No separate surrogate ID needed, matching the ERD.
        // Define the composite key right above the class
        
        public int SegmentId { get; set; }
        public int StopId { get; set; }

        // Order this stop appears in along the route (1 = first, 2 = second, etc.)
        public int StopSequence { get; set; }

        // Navigation Properties
        public virtual RouteSegment? RouteSegment { get; set; }
        public virtual Stop? Stop { get; set; }
    }
}