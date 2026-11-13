using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace UrbanLink_CRUD.Models.Entities.Transit
{
    // 1. Enforces Max 1 Card per User per Provider
    [Index(nameof(UserId), nameof(ProviderId), IsUnique = true, Name = "IX_TransitCards_User_Provider")]
    // 2. Enforces unique 10-digit card numbers system-wide
    [Index(nameof(OperatorCardNumber), IsUnique = true, Name = "IX_TransitCards_CardNumber")]
    public class ProviderCard
    {
        public int Id { get; set; }

        // LINK DIRECTLY TO USER (Bypass the general bank 'Card' table)
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int ProviderId { get; set; }
        public virtual TransitProvider Provider { get; set; } = null!;

        // The 10-digit UID (e.g., "3034223300")
        [MaxLength(10)]
        public string OperatorCardNumber { get; set; } = string.Empty;

        public decimal Balance { get; set; } = 0.00m;
        public bool IsActive { get; set; } = true;

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public string CryptoSeed { get; set; } = string.Empty;

        // Navigation Property: Link this to the top-ups we discussed!
        public virtual ICollection<CardTopUp> TopUps { get; set; } = new List<CardTopUp>();
    }
}