using UrbanLink_CRUD.Models.Entities.Transit;

namespace UrbanLink_CRUD.Models.Entities
{
    public class Card
    {
        public int Id { get; set; }
        
        // Foreign Key to User
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public string CardHolderName { get; set; } = string.Empty;
        public string MaskedCardNumber { get; set; } = string.Empty; // e.g., "**** **** **** 1234"
        public string ExpiryDate { get; set; } = string.Empty; // e.g., "MM/YY"
        
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true; // Soft deactivation support
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for provider-specific card linkages (if applicable)
        public virtual ICollection<TransitCard> TransitCards { get; set; } = new List<TransitCard>();
    }
}