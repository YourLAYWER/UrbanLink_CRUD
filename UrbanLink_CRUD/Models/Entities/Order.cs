using System.Data;
namespace UrbanLink_CRUD.Models.Entities 
{
    public class Order
    {
        // Primary Key
        public int Id { get; set; }

        // Foreign Key to User (Who bought it)
        public int UserId { get; set; }

        // Foreign Key to Retailer (Where it was bought)
        public int RetailerId { get; set; }

        public decimal TotalAmount { get; set; }
        public string FulfillmentType { get; set; } = "Delivery"; // e.g., "Delivery", "Collection"
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string? FulfillmentFeedback { get; set; } // Customer feedback/review text
        public int? Rating { get; set; }

        // --- Navigation Properties ---
        public virtual User? User { get; set; }
        public virtual Retailer? Retailer { get; set; }
    }
}

