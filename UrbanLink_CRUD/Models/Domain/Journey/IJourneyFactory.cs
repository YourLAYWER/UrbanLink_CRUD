using UrbanLink_CRUD.Models.Entities.Transit;
using RoutingLeg = UrbanLink_CRUD.Services.RoutingLogic.JourneyLeg;

namespace UrbanLink_CRUD.Models.Domain.Journey
{
    public interface IJourneyFactory
    {
        UserJourney CreateUserJourney(int userId, int startStopId, int endStopId, decimal totalCost, DateTime estimatedArrival, IEnumerable<RoutingLeg> legs);
        CashJourney CreateCashJourney(int shiftId, decimal fareAmount, int endStopId);
    }
}
