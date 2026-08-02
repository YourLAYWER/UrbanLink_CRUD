using System.Data;

namespace UrbanLink_CRUD.Models.Entities 
{
    public class Notification
    {
        // Primary Key
        public int Id { get; set; }

        // Foreign Key to User (Which user receives this notification)
        public int UserId { get; set; }

        public string Type { get; set; } = string.Empty;    // e.g., "TransitAlert", "ProximityAlert", "BudgetWarning"
        public string Message { get; set; } = string.Empty;
        // --- Navigation Property ---
        // Tells EF Core this notification belongs to ONE User
        public virtual User? User { get; set; }
    }
}

