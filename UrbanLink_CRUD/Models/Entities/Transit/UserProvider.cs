namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class UserProvider
    {
        public int Id { get; set; }

        // Foreign Key to User (Staff member)
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        // Foreign Key to TransitProvider (Employer / Agency)
        public int ProviderId { get; set; }
        public virtual TransitProvider Provider { get; set; } = null!;

        public bool IsActive { get; set; } = true;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}