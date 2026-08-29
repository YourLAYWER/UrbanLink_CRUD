namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{
    public class PrintedTicketDto
    {
        public string TicketId { get; set; } // e.g., "CASH-A8F92..."
        public string RouteName { get; set; }
        public decimal FarePaid { get; set; }
        public DateTime IssuedAt { get; set; }
    }
}
