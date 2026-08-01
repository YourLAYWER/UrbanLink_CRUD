using System.Data;

namespace UrbanLink_CRUD.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;


        // CommuterProfile can be (e.g ...,"Standard", "Student", or "Pensioner")
        public string CommuterProfile { get; set; } = "Standard";

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;


        // Navigation Properties ( A user has many tickets and orders)
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();


    }
}
