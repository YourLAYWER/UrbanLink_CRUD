using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class Ticket
    {
        // Primary Key
        public int TicketId { get; set; }

        public string QRCodeData { get; set; } = string.Empty;

        public TicketStatus Status { get; set; } = TicketStatus.Active;

        // Links a ticket to the route it was bought for, and therefore its provider
        public int? RouteId { get; set; }
        public virtual TransitRoute? Route { get; set; }

        public DateTime PurchasedAt { get; set; } = DateTime.Now;
    }
}