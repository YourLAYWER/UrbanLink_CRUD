using System.Data;

namespace UrbanLink_CRUD.Models.Entities.Aggregator
{
    public class Retailer
    {
        // Primary Key (Auto-Increment)
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public decimal DeliveryFee { get; set; }
        public bool CollectionAvailable { get; set; } = true;

        // --- Navigation Properties ---
        // One Retailer has MANY RetailerProducts (inventory pricing)
        public virtual ICollection<RetailerProduct> RetailerProducts { get; set; } = new List<RetailerProduct>();

        // One Retailer handles MANY Orders
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}

