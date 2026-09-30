namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{
    public class StopDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsTransferHub { get; set; }
        public bool IsActive { get; set; }

        // NEW: Array to hold all route IDs that service this stop
        public List<int> RouteIds { get; set; } = new List<int>();
    }
}
