namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class Shift
    {
        public int Id { get; set; }

        public int DriverUserId { get; set; }
        public int VehicleId { get; set; }
        public int? RouteSegmentId { get; set; } // nullable — a shift may not yet be assigned a route

        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }

        // "Active", "Ended", "ForceClosed"
        public string Status { get; set; } = "Active";

        // Navigation properties
        public virtual User? Driver { get; set; }
        public virtual Vehicle? Vehicle { get; set; }
        public virtual RouteSegment? RouteSegment { get; set; }
    }
}
