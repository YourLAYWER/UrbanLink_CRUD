using System;
namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{
    public class ActiveTripDto
    {
        public string CommuterName { get; set; } = string.Empty;
        public string CardId { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string CurrentStation { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}

