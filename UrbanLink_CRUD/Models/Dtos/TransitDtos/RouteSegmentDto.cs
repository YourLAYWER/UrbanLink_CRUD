namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{
    public class RouteSegmentDto
    {
        public int StopId { get; set; }
        public string StopName { get; set; }
        public string TransitLine { get; set; } // e.g., "PRASA Train - Line S1"
        public string ArrivalTime { get; set; }
    }
}
