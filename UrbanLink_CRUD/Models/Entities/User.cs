using System.Data;
using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;


        // CommuterProfile can be (e.g ...,"Standard", "Student", or "Pensioner")
        public Role Role { get; set; } = Role.StandardUser;

        public bool IsActive {get ; set ;} = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ==========================================
        // REFRESH TOKEN PROPERTIES
        // ==========================================

        // Stores the long-lived refresh token string
        public string? RefreshToken { get; set; }

        // Tracks when the refresh token becomes invalid
        public DateTime? RefreshTokenExpiryTime { get; set; }

    }
}
