namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{
    public class CashSaleRequest
    {
        public int ShiftId { get; set; }
        public int PassengerCount { get; set; } = 1;
    }
}
