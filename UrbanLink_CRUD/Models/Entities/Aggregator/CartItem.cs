namespace UrbanLink_CRUD.Models.Entities.Aggregator
{
    public class CartItem
    {
        // Primary Key
        public int CartItemID { get; set; }

        // Foreign Keys
        public int CartID { get; set; }

        public int ProductID { get; set; }

        public int Qty { get; set; }

        // Navigation Properties
        public virtual Cart? Cart { get; set; }

        public virtual Product? Product { get; set; }
    }
}