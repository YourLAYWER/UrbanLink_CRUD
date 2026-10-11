using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Entities.Transit;
using Microsoft.EntityFrameworkCore;

namespace UrbanLink_CRUD.Services.RoutingLogic
{
    public static class TransitGraphLoader
    {
        public static (List<GraphEdge> Edges, List<int> HubIds) Load(ULDBContext context, ILogger logger)
        {
            var segments = context.RouteSegments
                .AsNoTracking()
                .Include(s => s.Route)
                    .ThenInclude(r => r!.Schedules)
                .ToList();

            var edges = new List<GraphEdge>(segments.Count);

            foreach (var group in segments.GroupBy(s => s.RouteId))
            {
                int routeId = group.Key;
                int segmentCount = 0;

                // Resolve schedules ONCE per route (previously: a new List per edge).
                var route = group.First().Route;
                if (route is null)
                {
                    logger.LogWarning("Route {RouteId} has segments but no Route row; skipped.", routeId);
                    continue;
                }

                var (weekday, weekend) = ScheduleInfo.Resolve(route.Schedules);
                if (weekday is null && weekend is null)
                    logger.LogWarning("Route {RouteId} has no usable weekday/weekend schedule; it can never be boarded.", routeId);

                // Index by origin so the chain walk is O(n), not O(n^2).
                var byOrigin = new Dictionary<int, RouteSegment>();
                var destinations = new HashSet<int>();

                foreach (var seg in group)
                {
                    segmentCount++;
                    if (!byOrigin.TryAdd(seg.OriginStopId, seg))
                    {
                        logger.LogWarning(
                            "Route {RouteId}: stop {StopId} has more than one outgoing segment (branching is not supported); extra segment ignored.",
                            routeId, seg.OriginStopId);
                        continue;
                    }
                    destinations.Add(seg.DestinationStopId);
                }

                // The first segment is the one whose origin is nobody's destination.
                var current = byOrigin.Values.FirstOrDefault(s => !destinations.Contains(s.OriginStopId));
                if (current is null)
                {
                    logger.LogWarning("Route {RouteId} has no start segment (circular route?); skipped.", routeId);
                    continue;
                }

                var visited = new HashSet<int>();
                int offset = 0;

                // visited.Add guards against infinite loops if a stop repeats mid-route.
                while (current is not null && visited.Add(current.OriginStopId))
                {
                    edges.Add(new GraphEdge(
                        current.OriginStopId,
                        current.DestinationStopId,
                        current.RouteId,
                        current.TravelTimeMinutes,
                        route.BaseFare,
                        offset,
                        weekday,
                        weekend));

                    offset += current.TravelTimeMinutes;
                    byOrigin.TryGetValue(current.DestinationStopId, out current);
                }

                if (visited.Count < segmentCount)
                    logger.LogWarning(
                        "Route {RouteId}: only {Used} of {Total} segments were reachable from the start; the rest were dropped.",
                        routeId, visited.Count, segmentCount);
            }

            var hubIds = context.Stops
                .AsNoTracking()
                .Where(s => s.IsTransferHub && s.IsActive)   // a closed hub shouldn't allow transfers
                .Select(s => s.Id)
                .ToList();

            return (edges, hubIds);
        }

    }
}
