using UrbanLink_CRUD.Models.Entities.Transit;
using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Services.RoutingLogic
{
    // Schedules are precomputed per day type when the graph is built,
    // so the search never scans a List<RouteSchedule> or compares strings.
    public sealed record GraphEdge(
        int OriginStopId,
        int DestinationStopId,
        int RouteId,
        RouteDirections Direction,
        int TravelTimeMinutes,
        decimal RouteBaseFare,
        int OffsetFromRouteStart,
        ScheduleInfo? WeekdaySchedule,
        ScheduleInfo? WeekendSchedule)
    {
        // The "vehicle" a rider is sitting on. Outbound and inbound trips of the same
        // route are different vehicles, so switching between them counts as a transfer.
        // Always >= 0, so it never collides with the "not boarded yet" marker (-1).
        public int LineKey => RouteId * 2 + (int)Direction;

        public ScheduleInfo? ScheduleFor(DayType day) =>
            day == DayType.Weekend ? WeekendSchedule : WeekdaySchedule;
    }

    // Kept as-is (plus Direction) so the controller and journey factory keep working.
    public class JourneyLeg
    {
        public int RouteId { get; set; }
        public RouteDirections Direction { get; set; }
        public int StartStopId { get; set; }
        public int EndStopId { get; set; }
        public int LegDurationMinutes { get; set; }
        public decimal LegFare { get; set; }
    }

    public record RoutingResult(int TotalTimeMinutes, List<JourneyLeg> Legs);

}
