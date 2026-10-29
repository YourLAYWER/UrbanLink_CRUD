using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;

namespace UrbanLink_CRUD.Models.Domain.Payment
{
    public class BankCardPaymentStrategy : IPaymentStrategy
    {
        private readonly ULDBContext _context;

        public BankCardPaymentStrategy(ULDBContext context)
        {
            _context = context;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentContext ctx)
        {
            if (ctx.CardId == null)
                return new PaymentResult(false, "A valid Card ID is required for bank card payments.", string.Empty);

            // Ensure the card exists and belongs to the authenticated user
            var card = await _context.Cards
                .FirstOrDefaultAsync(c => c.Id == ctx.CardId && c.UserId == ctx.UserId && c.IsActive);

            if (card == null)
                return new PaymentResult(false, "Selected payment card is invalid or unauthorized.", string.Empty);

            // TODO: Execute HTTP call to Payment Gateway (Stripe/Paystack) here.
            // For now, we simulate a successful charge.
            
            string refId = $"EXT-BNK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            return new PaymentResult(true, string.Empty, refId);
        }
    }
}