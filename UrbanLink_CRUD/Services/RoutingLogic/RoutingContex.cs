namespace UrbanLink_CRUD.Services.RoutingLogic
{
    public record RoutingContex
    {
        public record RoutingContext(
        int StartStopId,
        int TargetStopId,
        DateTime UserDepartureTime
        );
    }
}
