using UrbanLink_CRUD.Models.Entities.Transit;

namespace UrbanLink_CRUD.Services.RoutingLogic
{
    public record GraphEdge(int OriginStopId, int DestinationStopId, int RouteId, int TravelTimeMinutes,
        decimal RouteBaseFare, int OffsetFromRouteStart, List<RouteSchedule> Schedules);

    public record SearchState(int StopId, int CurrentRouteId, int TotalTimeMinutes);

    public class JourneyLeg
    {
        public int RouteId { get; set; }
        public int StartStopId { get; set; }
        public int EndStopId { get; set; }
        public int LegDurationMinutes { get; set; }
        public decimal LegFare { get; set; }
    }

    public record RoutingResult(int TotalTimeMinutes, List<JourneyLeg> Legs);
    public class RoutingModels
    {
        
    }
}
