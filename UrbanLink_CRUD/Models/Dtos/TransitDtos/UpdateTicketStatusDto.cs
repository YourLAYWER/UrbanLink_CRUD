namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{
    public class UpdateTicketStatusDto
    {
        public Guid TicketReference { get; set; }
        public string Status { get; set; } // e.g., "REDEEMED" or "EXPIRED"
    }
}
