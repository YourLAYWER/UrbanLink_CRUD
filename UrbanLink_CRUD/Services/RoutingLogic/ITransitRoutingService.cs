namespace UrbanLink_CRUD.Services.RoutingLogic
{
    public interface ITransitRoutingService
    {
        void InitializeGraph(IEnumerable<GraphEdge> edges, IEnumerable<int> transferHubIds);
        RoutingResult ComputeRoute(int startStopId, int targetStopId, DateTime userDepartureTime, string preference = "fastest");
    }
}
