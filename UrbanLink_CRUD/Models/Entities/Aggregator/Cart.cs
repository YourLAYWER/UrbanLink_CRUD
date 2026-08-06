using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Models.Entities.Aggregator
{
    public class Cart
    {
        // Primary Key
        public int CartID { get; set; }

        // Foreign Key
        public int UserId { get; set; }

        public CartStatus Status { get; set; } = CartStatus.Active;

        // Navigation Properties
        public virtual User? User { get; set; }

        public virtual ICollection<CartItem>? CartItems { get; set; }
    }
}