namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{
    public class BusLocationUpdateDto
    {
        public int BusId { get; set; }
        public int RouteId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}