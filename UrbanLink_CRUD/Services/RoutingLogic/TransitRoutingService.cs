namespace UrbanLink_CRUD.Services.RoutingLogic
{
    public class TransitRoutingService : ITransitRoutingService
    {
        // Graph and hubs live in one immutable object and are swapped with a single
        // reference write, so a request never sees a graph paired with the wrong hubs.
        private sealed record Snapshot(
            Dictionary<int, List<GraphEdge>> Graph,
            HashSet<int> Hubs);

        private volatile Snapshot _snapshot = new(new(), new());
        private readonly IRoutingStrategyFactory _strategyFactory;

        public TransitRoutingService(IRoutingStrategyFactory strategyFactory)
        {
            _strategyFactory = strategyFactory;
        }

        public void InitializeGraph(IEnumerable<GraphEdge> edges, IEnumerable<int> transferHubIds)
        {
            var graph = new Dictionary<int, List<GraphEdge>>();

            foreach (var edge in edges)
            {
                if (!graph.TryGetValue(edge.OriginStopId, out var list))
                    graph[edge.OriginStopId] = list = new List<GraphEdge>();

                list.Add(edge);
            }

            _snapshot = new Snapshot(graph, new HashSet<int>(transferHubIds));
        }

        public RoutingResult ComputeRoute(
            int startStopId,
            int targetStopId,
            DateTime userDepartureTime,
            string preference = "fastest")
        {
            var snap = _snapshot;   // read once; use only this local for the whole request
            var strategy = _strategyFactory.GetStrategy(preference);
            var context = new RoutingContext(startStopId, targetStopId, userDepartureTime);

            return strategy.ComputeRoute(context, snap.Graph, snap.Hubs);
        }

    }
}
