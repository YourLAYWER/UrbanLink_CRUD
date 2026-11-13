namespace UrbanLink_CRUD.Models.Dtos.TransitDtos
{
    public class NfcTapRequest
    {
        // The raw string broadcast by the phone over NFC
        // Expected format: "3034223300.A7F9C2B4E8..." (CardNumber.Hash)
        public string NfcPayload { get; set; }

        // The ID of the shift/conductor scanning the card
        public int ConductorId { get; set; }
    }
}

