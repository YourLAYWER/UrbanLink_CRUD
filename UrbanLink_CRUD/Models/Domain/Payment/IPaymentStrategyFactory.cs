namespace UrbanLink_CRUD.Models.Domain.Payment
{
    public interface IPaymentStrategyFactory
    {
        IPaymentStrategy GetStrategy(string paymentMethod);
    }
}