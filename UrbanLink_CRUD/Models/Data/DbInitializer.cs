using BCrypt.Net;
using UrbanLink_CRUD.Models.Entities;

namespace UrbanLink_CRUD.Models.Data
{
    public class DbInitializer
    {
        // 1. Check if the admin already exists so we don't duplicate it on every run
        public static void SeedAdminUser(ULDBContext context)
        {
            // 1. Check if the admin already exists so we don't duplicate it on every run
            if (!context.Users.Any(u => u.Email == "admin@urbanlink.co.za"))
            {
                // 2. Hash the default admin password
                string adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("AdminSecure@2026!", 11);

                // 3. Create the admin user
                var adminUser = new User
                {
                    Name = "System Administrator",
                    Email = "admin@urbanlink.co.za",
                    Password = adminPasswordHash,
                    CommuterProfile = "Admin", // This is the crucial role for your [Authorize] tags
                    CreatedAt = DateTime.UtcNow
                };

                // 4. Save to the database
                context.Users.Add(adminUser);
                context.SaveChanges();
            }
        }
    }

}
