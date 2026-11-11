namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{
    public class UserWalletResponseDto
    {
        public IEnumerable<CardResponseDto> PaymentCards { get; set; } = new List<CardResponseDto>();
        public IEnumerable<TransitCardResponseDto> TransitCards { get; set; } = new List<TransitCardResponseDto>();
    }
}