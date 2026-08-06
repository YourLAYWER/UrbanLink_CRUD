namespace UrbanLink_CRUD.Models.Entities.Aggregator
{
    public class OrderItem
    {
        // Primary Key
        public int OrderItemID { get; set; }

        public int Quantity { get; set; }

        // Foreign Keys
        public int OrderID { get; set; }

        public int ProductID { get; set; }

        // Navigation Properties
        public virtual Order? Order { get; set; }

        public virtual Product? Product { get; set; }
    }
}