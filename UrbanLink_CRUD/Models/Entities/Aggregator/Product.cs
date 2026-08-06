using System.Collections.Generic;

namespace UrbanLink_CRUD.Models.Entities.Aggregator
{
    public class Product
    {
        // Primary Key
        public int ProductID { get; set; }

        public string UniversalName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string ImageURL { get; set; } = string.Empty;

        // Navigation Properties
        public virtual ICollection<OrderItem>? OrderItems { get; set; }

        public virtual ICollection<CartItem>? CartItems { get; set; }
    }
}