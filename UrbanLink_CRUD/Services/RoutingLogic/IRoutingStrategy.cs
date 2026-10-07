using static UrbanLink_CRUD.Services.RoutingLogic.RoutingContex;

namespace UrbanLink_CRUD.Services.RoutingLogic
{
    public interface IRoutingStrategy
    {
        RoutingResult ComputeRoute(
            RoutingContext context,
            IReadOnlyDictionary<int, List<GraphEdge>> graph,
            IReadOnlySet<int> transferHubs
        );
    }
}
