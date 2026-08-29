namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{
    public class ScanTicketRequest
    {
        public int ShiftId { get; set; }
        public string QRCodeData {  get; set; }
    }
}
