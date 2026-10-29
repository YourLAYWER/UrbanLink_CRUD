using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;

namespace UrbanLink_CRUD.Models.Domain.Payment
{
    public class ProviderWalletPaymentStrategy : IPaymentStrategy
    {
        private readonly ULDBContext _context;

        public ProviderWalletPaymentStrategy(ULDBContext context)
        {
            _context = context;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentContext ctx)
        {
            // Find the user's active TransitCard (Provider Wallet)
            var transitCard = await _context.TransitCards
                .Include(tc => tc.Card)
                .FirstOrDefaultAsync(tc => tc.Card.UserId == ctx.UserId && tc.IsActive);

            if (transitCard == null)
                return new PaymentResult(false, "No active UrbanLink wallet found for this user.", string.Empty);

            if (transitCard.Balance < ctx.Amount)
                return new PaymentResult(false, $"Insufficient wallet balance. Fare is R{ctx.Amount:0.00} but balance is R{transitCard.Balance:0.00}.", string.Empty);

            // Deduct the balance
            transitCard.Balance -= ctx.Amount;
            
            // Transaction reference for internal auditing
            string refId = $"URB-WLT-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            
            return new PaymentResult(true, string.Empty, refId);
        }
    }
}