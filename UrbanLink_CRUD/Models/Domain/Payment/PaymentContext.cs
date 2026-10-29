namespace UrbanLink_CRUD.Models.Domain.Payment
{
    public record PaymentContext(
        int UserId, 
        decimal Amount, 
        int? CardId // Will be null if they use the default UrbanLink Provider Wallet
    );
}