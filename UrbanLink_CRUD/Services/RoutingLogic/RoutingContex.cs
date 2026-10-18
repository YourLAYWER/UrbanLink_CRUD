namespace UrbanLink_CRUD.Services.RoutingLogic
{
        public readonly record struct RoutingContext(
        int StartStopId,
        int TargetStopId,
        DateTime UserDepartureTime);

}
