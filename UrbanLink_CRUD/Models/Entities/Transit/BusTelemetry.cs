using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class BusTelemetry
    {
        public long Id { get; set; } // Using long (bigint) because this table will grow massive over time

        public required int BusId { get; set; }

        public required int RouteId { get; set; }

        // Decimal(10,8) allows for high-precision GPS hardware coordinates
        [Column(TypeName = "decimal(10, 8)")]
        public required double Latitude { get; set; }

        [Column(TypeName = "decimal(11, 8)")]
        public required double Longitude { get; set; }

        public required DateTime Timestamp { get; set; }
    }
}