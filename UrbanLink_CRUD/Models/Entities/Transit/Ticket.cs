using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class Ticket
    {
        // Primary Key
        public int TicketId { get; set; }

        public string QRCodeData { get; set; } = string.Empty;

        public TicketStatus Status { get; set; } = TicketStatus.Active;
    }
}