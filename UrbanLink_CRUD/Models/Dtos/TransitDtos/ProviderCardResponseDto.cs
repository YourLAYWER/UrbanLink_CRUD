namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{
    public class TransitCardResponseDto
    {
        public int Id { get; set; }
        public int ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string OperatorCardNumber { get; set; }
        public decimal Balance { get; set; }
        public DateTime IssuedAt { get; set; }
        public string CryptoSeed { get; set; }
    }
}