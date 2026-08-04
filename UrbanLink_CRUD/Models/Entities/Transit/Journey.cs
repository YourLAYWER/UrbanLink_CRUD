using Microsoft.Extensions.Configuration.UserSecrets;

namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class Journey
    {
        public int ID { get; set; }
        public int userID { get; set; }
        public string StartStop { get; set; } = string.Empty;
        public string EndStop { get; set; } = string.Empty;
        public decimal TotalCost { get; set; }
        public string Status { get; set; } = "Active"; // Possible values: "Active", "Completed", "Cancelled"

        // Security fields
        public Guid TicketReference { get; set; } = Guid.NewGuid(); // Unique reference for the ticket

        // Tracks the exact second the conductor scanned the ticket (Nullable until scanned)
        public DateTime? ScanTimestamp { get; set; }


        // Nullable fields for when the trip is finished
        public int? Rating { get; set; }
        public string? FeedbackTag { get; set; }
        public string? ReviewNotes { get; set; }


        // Navigation Properties
        public virtual User? User { get; set; }
    }
}
