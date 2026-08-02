using System.Data;

namespace UrbanLink_CRUD.Models.Entities;

public class RetailerProduct
{
    // Primary Key
    public int Id { get; set; }

    // Foreign Key to Retailer (The [MANY] side)
    public int RetailerId { get; set; }

    // Foreign Key to Product (The [MANY] side)
    //public int ProductId { get; set; }

    public decimal CurrentPrice { get; set; }
    public bool InStock { get; set; } = true;

    // --- Navigation Properties ---
    public virtual Retailer? Retailer { get; set; }
    //public virtual Product? Product { get; set; }
}

