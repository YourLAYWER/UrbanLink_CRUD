namespace UrbanLink_CRUD.Models.Domain.Payment
{
    public record PaymentResult(
        bool IsSuccess, 
        string ErrorMessage, 
        string TransactionReference
    );
}