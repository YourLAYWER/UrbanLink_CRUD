using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{
    public class IncidentReportRequestDto
    {
        public bool IsEmergency { get; set; }

        // Android will send an integer matching your C# enum index
        public IncidentCategory Category { get; set; }

        public string Description { get; set; }

        // If the device can't get a GPS lock, these default to 0.0
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
