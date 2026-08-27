namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class IncidentReport
    {
        public int Id { get; set; }

        // Link directly to the active shift
        public int ConductorShiftId { get; set; }
        public ConductorShift ConductorShift { get; set; }

        public DateTime ReportedAt { get; set; }

        // Critical for the SOS button
        public bool IsEmergency { get; set; }

        // e.g., "Engine Trouble", "Passenger Dispute", or "SOS"
        public string Category { get; set; }

        // The optional text/voice note
        public string Description { get; set; }

        // GPS Coordinates sent from the Android device
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // E.g., "Open", "Investigating", "Resolved"
        public string Status { get; set; }
    }
}
