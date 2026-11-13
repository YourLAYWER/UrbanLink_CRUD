using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UrbanLink_CRUD.Models.Dtos.TransitDtos;
using UrbanLink_CRUD.Models.Entities;
using UrbanLink_CRUD.Models.Entities.Transit;
using UrbanLink_CRUD.Repositories;
using UrbanLink_CRUD.Models.Data;
using System.Security.Cryptography;

namespace UrbanLink_CRUD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CardsController : ControllerBase
    {
        private readonly ICardRepository _cardRepository;
        private readonly ULDBContext _context;

        public CardsController(ICardRepository cardRepository, ULDBContext context)
        {
            _cardRepository = cardRepository;
            _context = context;
        }

        // Helper to extract the authenticated User ID from the JWT
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyCards()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            // 1. Fetch standard Payment Cards (Debit/Credit) via existing repo
            var bankCards = await _cardRepository.GetActiveCardsByUserIdAsync(userId);
            var paymentCardResponses = bankCards.Select(c => new CardResponseDto
            {
                Id = c.Id,
                CardHolderName = c.CardHolderName,
                MaskedCardNumber = c.MaskedCardNumber,
                ExpiryDate = c.ExpiryDate,
                IsDefault = c.IsDefault
            });

            // 2. Fetch Transit Provider Cards via EF Core
            var transitCards = await _context.ProviderCards
                .Include(tc => tc.Provider)
                .Where(tc => tc.UserId == userId && tc.IsActive)
                .Select(tc => new TransitCardResponseDto
                {
                    Id = tc.Id,
                    ProviderId = tc.ProviderId,
                    ProviderName = tc.Provider.Name,
                    OperatorCardNumber = tc.OperatorCardNumber,
                    Balance = tc.Balance,
                    IssuedAt = tc.IssuedAt
                })
                .ToListAsync();

            // 3. Combine them into a single Wallet response
            var walletResponse = new UserWalletResponseDto
            {
                PaymentCards = paymentCardResponses,
                TransitCards = transitCards
            };

            return Ok(walletResponse);
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateDigitalCard([FromBody] GenerateWalletRequest request)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            // 1. Verify Provider Exists
            var providerExists = await _context.TransitProviders.AnyAsync(p => p.Id == request.ProviderId);
            if (!providerExists)
            {
                return NotFound(new { message = "Transit provider not found." });
            }

            // 2. Enforce constraint: Max 1 card per provider per user
            var existingCard = await _context.ProviderCards
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProviderId == request.ProviderId);

            if (existingCard != null)
            {
                return BadRequest(new { message = "You already have an active card for this provider." });
            }

            // 3. Mint unique 10-digit serial number
            string newCardNumber = string.Empty;
            bool isUnique = false;
            Random rnd = new Random();

            while (!isUnique)
            {
                // Combine two random 5-digit chunks
                int part1 = rnd.Next(10000, 100000);
                int part2 = rnd.Next(10000, 100000);
                newCardNumber = $"{part1}{part2}";

                // Verify against existing physical and digital cards in the system
                isUnique = !await _context.ProviderCards.AnyAsync(c => c.OperatorCardNumber == newCardNumber);
            }

            // 4. Save the new digital wallet

            var newCard = new ProviderCard
            {
                UserId = userId,
                ProviderId = request.ProviderId,
                OperatorCardNumber = newCardNumber,
                Balance = 0.00m,
                IsActive = true,
                IssuedAt = DateTime.UtcNow,
                CryptoSeed = GenerateSecureCryptoSeed()
            };

            _context.ProviderCards.Add(newCard);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Digital card generated successfully.",
                cardNumber = newCard.OperatorCardNumber,
                balance = newCard.Balance,
                providerId = newCard.ProviderId
            });
        }

        private string GenerateSecureCryptoSeed()
        {
            var key = new byte[32];
            RandomNumberGenerator.Fill(key);
            return Convert.ToBase64String(key);
        }

        [HttpPost]
        public async Task<IActionResult> AddCard([FromBody] AddCardRequestDto request)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var newCard = new Card
            {
                UserId = userId,
                CardHolderName = request.CardHolderName,
                MaskedCardNumber = request.MaskedCardNumber,
                ExpiryDate = request.ExpiryDate,
                IsDefault = request.SetAsDefault
            };

            var savedCard = await _cardRepository.AddCardAsync(newCard);

            var response = new CardResponseDto
            {
                Id = savedCard.Id,
                CardHolderName = savedCard.CardHolderName,
                MaskedCardNumber = savedCard.MaskedCardNumber,
                ExpiryDate = savedCard.ExpiryDate,
                IsDefault = savedCard.IsDefault
            };

            return CreatedAtAction(nameof(GetMyCards), new { id = response.Id }, response);
        }

        [HttpPut("{id}/default")]
        public async Task<IActionResult> SetDefaultCard(int id)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            bool success = await _cardRepository.SetDefaultCardAsync(userId, id);

            if (!success) return NotFound("Card not found or already inactive.");

            return Ok(new { message = "Default payment method updated." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCard(int id)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            bool success = await _cardRepository.SoftDeleteCardAsync(userId, id);

            if (!success) return NotFound("Card not found.");

            return Ok(new { message = "Card removed successfully." });
        }
    }
}