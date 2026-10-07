using static UrbanLink_CRUD.Services.RoutingLogic.RoutingContex;

namespace UrbanLink_CRUD.Services.RoutingLogic
{
    public class TransitRoutingService : ITransitRoutingService
    {
        private Dictionary<int, List<GraphEdge>> _graph = new();
        private HashSet<int> _transferHubs = new();
        private readonly IRoutingStrategyFactory _strategyFactory;

        public TransitRoutingService(IRoutingStrategyFactory strategyFactory)
        {
            _strategyFactory = strategyFactory;
        }

        public void InitializeGraph(IEnumerable<GraphEdge> edges, IEnumerable<int> transferHubIds)
        {
            var newGraph = new Dictionary<int, List<GraphEdge>>();
            var newHubs = new HashSet<int>(transferHubIds);

            foreach (var edge in edges)
            {
                if (!newGraph.ContainsKey(edge.OriginStopId))
                    newGraph[edge.OriginStopId] = new List<GraphEdge>();

                newGraph[edge.OriginStopId].Add(edge);
            }

            _graph = newGraph;
            _transferHubs = newHubs;
        }

        public RoutingResult ComputeRoute(int startStopId, int targetStopId, DateTime userDepartureTime, string preference = "fastest")
        {
            var strategy = _strategyFactory.GetStrategy(preference);
            var context = new RoutingContext(startStopId, targetStopId, userDepartureTime);

            return strategy.ComputeRoute(context, _graph, _transferHubs);
        }
    }
}
