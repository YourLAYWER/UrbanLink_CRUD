using UrbanLink_CRUD.Models.Entities.Transit;
using UrbanLink_CRUD.Models.Enums;
using RoutingLeg = UrbanLink_CRUD.Services.RoutingLogic.JourneyLeg;

namespace UrbanLink_CRUD.Models.Domain.Journey
{
    public class JourneyFactory : IJourneyFactory
    {
        public UserJourney CreateUserJourney(int userId, int startStopId, int endStopId, decimal totalCost, DateTime estimatedArrival, IEnumerable<RoutingLeg> legs)
        {

            var userJourney = new UserJourney
            {
                UserID = userId,
                StartStopID = startStopId,
                EndStopID = endStopId,
                TotalCost = totalCost,
                EstimatedArrivalTime = estimatedArrival,
                Status = JourneyStatus.Active,
                CreatedAt = DateTime.UtcNow,
                TicketReference = Guid.NewGuid(),
                Legs = new List<UrbanLink_CRUD.Models.Entities.Transit.JourneyLeg>()
            };

            int sequenceOrder = 1;
            foreach (var leg in legs)
            {
                userJourney.Legs.Add(new JourneyLeg
                {
                    SequenceOrder = sequenceOrder++,
                    RouteId = leg.RouteId,
                    BoardingStopId = leg.StartStopId,         // Maps routing leg's start stop to BoardingStopId
                    DestinationStopId = leg.EndStopId,       // Maps routing leg's end stop to DestinationStopId
                    LegDurationMinutes = leg.LegDurationMinutes,
                    LegFare = leg.LegFare,
                    Status = IncidentStatus.Open,
                    TicketReference = Guid.NewGuid()
                });
            }

            return userJourney;
        }
        public CashJourney CreateCashJourney(int shiftId, decimal fareAmount, int endStopId)
        {
            return new CashJourney
            {
                ShiftID = shiftId,
                TotalCost = fareAmount,
                EndStopID = endStopId,
                Status = JourneyStatus.Completed, // Cash journeys are instantly completed upon printing
                CreatedAt = DateTime.UtcNow,
                RedeemedAt = DateTime.UtcNow, // Redeemed the moment it's printed
                TicketReference = Guid.NewGuid()
            };
        }
    }
}
