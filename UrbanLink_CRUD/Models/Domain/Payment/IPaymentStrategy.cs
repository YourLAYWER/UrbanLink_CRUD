namespace UrbanLink_CRUD.Models.Domain.Payment
{
    public interface IPaymentStrategy
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentContext context);
    }
}