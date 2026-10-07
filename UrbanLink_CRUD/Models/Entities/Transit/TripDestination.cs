namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class TripDestination
    {
        public int Id { get; set; }

        public int ShiftId { get; set; }
        // Optional: Navigation property to ConductorShift if needed
        // public ConductorShift Shift { get; set; } 

        public int StopId { get; set; }
        public Stop Stop { get; set; } // Required for the .Include(td => td.Stop)

        public string Status { get; set; } // e.g., "QUEUED", "CLEARED"

        public int PassengersDroppingOff { get; set; } = 1;
    }
}
