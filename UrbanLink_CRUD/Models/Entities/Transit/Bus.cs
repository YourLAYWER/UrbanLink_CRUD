using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class Bus
    {
        public int Id { get; set; }

        // The internal company code (e.g., "ALGOA-042" or "NMBM-105")
        public required string FleetNumber { get; set; }

        // The actual government registration plate (e.g., "CAA 12345")
        public required string LicensePlate { get; set; }

        // Crucial for dynamic routing: Total capacity = Seated + Standing
        public int SeatingCapacity { get; set; }
        public int StandingCapacity { get; set; }

        // "Active", "InMaintenance", "OutOfService"
        public BusStatus Status {  get; set; }

        // Foreign Key: Which provider owns this bus? (Algoa Bus, etc.)
        public int ProviderId { get; set; }
        public TransitProvider Provider { get; set; }

        public ICollection<ConductorShift> ConductorShifts { get; set; }
    }
}
