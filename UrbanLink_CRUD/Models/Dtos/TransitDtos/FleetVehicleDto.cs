using System;
namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{
    public class FleetVehicleDto
    {
        public string Id { get; set; } = string.Empty;
        public string UnitCode { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public string Status { get; set; } = "IN_SERVICE"; // e.g. "IN_SERVICE" or "MAINTENANCE"
        public int FuelBatteryLevel { get; set; }
        public bool IsFixed { get; set; }
        public string Route { get; set; } = string.Empty;
    }
}
