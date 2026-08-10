namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{
    public class RouteCalculateResponse
    {
        public decimal BaseCost { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public List<RouteSegmentDto> Segments { get; set; } = new List<RouteSegmentDto>();
    }
}
