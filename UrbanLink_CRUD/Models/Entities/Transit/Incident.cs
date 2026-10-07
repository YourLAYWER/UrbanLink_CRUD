namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class Incident
    {
        public int Id { get; set; }
        public int ReportedByUserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Open";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? AdminNotes { get; set; }

        public virtual User? ReportedBy { get; set; }
    }
}