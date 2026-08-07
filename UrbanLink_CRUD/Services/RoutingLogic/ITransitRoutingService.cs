namespace UrbanLink_CRUD.Services.RoutingLogic
{
    public interface ITransitRoutingService
    {
        void InitializeGraph(IEnumerable<GraphEdge> edges, IEnumerable<int> transferHubIds);
        RoutingResult ComputeFastestRoute(int startStopId, int targetStopId);
    }
}
