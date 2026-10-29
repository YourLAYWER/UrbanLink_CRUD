namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{
    public class BookingRequestDto
    {
        public int StartStopId { get; set; }
        public int EndStopId { get; set; }

        // Identifies the strategy ("SavedCard", "ProviderCard")
        public string PaymentMethod { get; set; } = string.Empty; 
        
        // Captures the exact card if "SavedCard" is used
        public int? CardId { get; set; }

        // public string Preference { get; set; }
    }
}
