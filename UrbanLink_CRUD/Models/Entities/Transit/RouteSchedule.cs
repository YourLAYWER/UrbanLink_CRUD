using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class RouteSchedule
    {
        [Key]
        public int ScheduleId { get; set; }

        // The Foreign Key connecting to your existing Routes table
        [ForeignKey("TransitRoute")]
        public int RouteId { get; set; }

        // EF Core Navigation Property (Allows you to easily query connected Route data)
        public TransitRoute TransitRoute { get; set; }

        public RouteDirections Direction { get; set; } = RouteDirections.Outbound;
        public required string DaysOfOperation { get; set; }

        public required TimeSpan FirstDepartureTime { get; set; }

        public required TimeSpan LastDepartureTime { get; set; }

        public required int FrequencyMinutes { get; set; }
    }
}
