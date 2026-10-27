namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class TransitCard
    {
        public int Id { get; set; }

        // Links to the general Card entity
        public int CardId { get; set; }
        public virtual Card Card { get; set; } = null!;

        // Links to the specific Transit Provider that issued/manages the card credit
        public int ProviderId { get; set; }
        public virtual TransitProvider Provider { get; set; } = null!;

        public string OperatorCardNumber { get; set; } = string.Empty; // Unique serial number on the physical transit card
        public decimal Balance { get; set; } = 0.00m; // Credit balance held on the provider card
        public bool IsActive { get; set; } = true;
        
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    }
}