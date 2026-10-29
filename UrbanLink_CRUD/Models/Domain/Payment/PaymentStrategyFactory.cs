using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;

namespace UrbanLink_CRUD.Models.Domain.Payment
{
    public class PaymentStrategyFactory : IPaymentStrategyFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public PaymentStrategyFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IPaymentStrategy GetStrategy(string paymentMethod)
        {
            return paymentMethod switch
            {
                "ProviderCard" => _serviceProvider.GetRequiredService<ProviderWalletPaymentStrategy>(),
                "SavedCard" => _serviceProvider.GetRequiredService<BankCardPaymentStrategy>(),
                _ => throw new ArgumentException($"Unsupported payment method: {paymentMethod}")
            };
        }
    }
}