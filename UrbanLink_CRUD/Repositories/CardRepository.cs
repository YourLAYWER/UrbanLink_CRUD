using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Entities;

namespace UrbanLink_CRUD.Repositories
{
    public interface ICardRepository
    {
        Task<IEnumerable<Card>> GetActiveCardsByUserIdAsync(int userId);
        Task<Card> AddCardAsync(Card card);
        Task<bool> SetDefaultCardAsync(int userId, int cardId);
        Task<bool> SoftDeleteCardAsync(int userId, int cardId);
    }

    public class CardRepository : ICardRepository
    {
        private readonly ULDBContext _context;

        public CardRepository(ULDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Card>> GetActiveCardsByUserIdAsync(int userId)
        {
            return await _context.Cards
                .Where(c => c.UserId == userId && c.IsActive)
                .OrderByDescending(c => c.IsDefault) // Default card appears first
                .ThenByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Card> AddCardAsync(Card card)
        {
            if (card.IsDefault)
            {
                await RemoveExistingDefaultAsync(card.UserId);
            }
            else
            {
                // If this is their first active card, force it to be the default
                bool hasActiveCards = await _context.Cards.AnyAsync(c => c.UserId == card.UserId && c.IsActive);
                if (!hasActiveCards) card.IsDefault = true;
            }

            _context.Cards.Add(card);
            await _context.SaveChangesAsync();
            return card;
        }

        public async Task<bool> SetDefaultCardAsync(int userId, int cardId)
        {
            var targetCard = await _context.Cards
                .FirstOrDefaultAsync(c => c.Id == cardId && c.UserId == userId && c.IsActive);

            if (targetCard == null) return false;

            await RemoveExistingDefaultAsync(userId);

            targetCard.IsDefault = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteCardAsync(int userId, int cardId)
        {
            var card = await _context.Cards
                .FirstOrDefaultAsync(c => c.Id == cardId && c.UserId == userId && c.IsActive);

            if (card == null) return false;

            card.IsActive = false;
            card.IsDefault = false; 

            // Optional: If they deleted their default card, randomly assign a new default
            var nextAvailableCard = await _context.Cards
                .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive && c.Id != cardId);
            
            if (nextAvailableCard != null)
            {
                nextAvailableCard.IsDefault = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task RemoveExistingDefaultAsync(int userId)
        {
            var existingDefault = await _context.Cards
                .FirstOrDefaultAsync(c => c.UserId == userId && c.IsDefault && c.IsActive);
            
            if (existingDefault != null)
            {
                existingDefault.IsDefault = false;
            }
        }
    }
}