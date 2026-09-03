using UrbanLink_CRUD.Models.Entities.Transit;

namespace UrbanLink_CRUD.Models.Domain.Journey
{
    public interface IJourneyFactory
    {
        UserJourney CreateUserJourney(int userId, int startStopId, int endStopId, decimal totalCost, DateTime estimatedArrival);
        CashJourney CreateCashJourney(int shiftId, decimal fareAmount, int endStopId);
    }
}
