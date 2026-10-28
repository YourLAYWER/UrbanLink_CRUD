using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UrbanLink_CRUD.Models.Dtos.TransitDtos;
using UrbanLink_CRUD.Models.Entities;
using UrbanLink_CRUD.Repositories;

namespace UrbanLink_CRUD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CardsController : ControllerBase
    {
        private readonly ICardRepository _cardRepository;

        public CardsController(ICardRepository cardRepository)
        {
            _cardRepository = cardRepository;
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

            var cards = await _cardRepository.GetActiveCardsByUserIdAsync(userId);
            
            var response = cards.Select(c => new CardResponseDto
            {
                Id = c.Id,
                CardHolderName = c.CardHolderName,
                MaskedCardNumber = c.MaskedCardNumber,
                ExpiryDate = c.ExpiryDate,
                IsDefault = c.IsDefault
            });

            return Ok(response);
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