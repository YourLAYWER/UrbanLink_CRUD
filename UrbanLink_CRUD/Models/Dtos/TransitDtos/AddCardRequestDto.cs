namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{
    public class AddCardRequestDto
    {
        public string CardHolderName { get; set; } = string.Empty;
        
        // In a real production app, you would send a secure token from Stripe/Paystack here.
        // For this implementation, we accept the masked number directly from the frontend UI.
        public string MaskedCardNumber { get; set; } = string.Empty; 
        public string ExpiryDate { get; set; } = string.Empty;
        public bool SetAsDefault { get; set; }
    }

    public class CardResponseDto
    {
        public int Id { get; set; }
        public string CardHolderName { get; set; } = string.Empty;
        public string MaskedCardNumber { get; set; } = string.Empty;
        public string ExpiryDate { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        // We omit mapping IsActive or CreatedAt to keep the mobile payload lightweight
    }
}