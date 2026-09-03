using UrbanLink_CRUD.Models.Entities.Transit;
using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Models.Domain.Journey
{
    public class JourneyFactory : IJourneyFactory
    {
        public UserJourney CreateUserJourney(int userId, int startStopId, int endStopId, decimal totalCost, DateTime estimatedArrival)
        {
            return new UserJourney
            {
                UserID = userId,
                StartStopID = startStopId,
                EndStopID = endStopId,
                TotalCost = totalCost,
                EstimatedArrivalTime = estimatedArrival,
                Status = JourneyStatus.Active,
                CreatedAt = DateTime.UtcNow,
                TicketReference = Guid.NewGuid()
            };
        }
        public CashJourney CreateCashJourney(int shiftId, decimal fareAmount, int endStopId)
        {
            return new CashJourney
            {
                ShiftID = shiftId,
                TotalCost = fareAmount,
                EndStopID= endStopId,
                Status = JourneyStatus.Completed, // Cash journeys are instantly completed upon printing
                CreatedAt = DateTime.UtcNow,
                RedeemedAt = DateTime.UtcNow, // Redeemed the moment it's printed
                TicketReference = Guid.NewGuid()
            };
        }
    }
}
