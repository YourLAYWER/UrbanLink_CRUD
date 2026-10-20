using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Entities.Transit;
using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Enums;
using UrbanLink_CRUD.Models.Enums;

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

            var edges = new List<GraphEdge>(segments.Count * 2);

            foreach (var group in segments.GroupBy(s => s.RouteId))
            {
                int routeId = group.Key;
                int segmentCount = 0;

                var route = group.First().Route;
                if (route is null)
                {
                    logger.LogWarning("Route {RouteId} has segments but no Route row; skipped.", routeId);
                    continue;
                }

                // Resolve schedules ONCE per route and direction.
                var (outWeekday, outWeekend) = ScheduleInfo.Resolve(route.Schedules, RouteDirections.Outbound);
                if (outWeekday is null && outWeekend is null)
                    logger.LogWarning("Route {RouteId} has no usable outbound weekday/weekend schedule; it can never be boarded outbound.", routeId);

                (ScheduleInfo? Weekday, ScheduleInfo? Weekend) inbound = (null, null);
                if (route.IsBidirectional)
                {
                    inbound = ScheduleInfo.Resolve(route.Schedules, RouteDirections.Inbound);
                    if (inbound.Weekday is null && inbound.Weekend is null)
                        logger.LogWarning("Route {RouteId} is bidirectional but has no usable inbound schedule; the return direction is disabled.", routeId);
                }

                // Index by origin so the chain walk is O(n), not O(n^2).
                // Only the FORWARD direction is stored in the database; the reverse is generated below.
                var byOrigin = new Dictionary<int, RouteSegment>();
                var destinations = new HashSet<int>();

                foreach (var seg in group)
                {
                    segmentCount++;
                    if (!byOrigin.TryAdd(seg.OriginStopId, seg))
                    {
                        logger.LogWarning(
                            "Route {RouteId}: stop {StopId} has more than one outgoing segment (branching is not supported). " +
                            "If this is a two-way route, store only one direction and set IsBidirectional instead; extra segment ignored.",
                            routeId, seg.OriginStopId);
                        continue;
                    }
                    destinations.Add(seg.DestinationStopId);
                }

                var current = byOrigin.Values.FirstOrDefault(s => !destinations.Contains(s.OriginStopId));
                if (current is null)
                {
                    logger.LogWarning("Route {RouteId} has no start segment (circular route?); skipped.", routeId);
                    continue;
                }

                var chain = new List<RouteSegment>();
                var visited = new HashSet<int>();
                int offset = 0;

                // visited.Add guards against infinite loops if a stop repeats mid-route.
                while (current is not null && visited.Add(current.OriginStopId))
                {
                    chain.Add(current);

                    edges.Add(new GraphEdge(
                        current.OriginStopId,
                        current.DestinationStopId,
                        current.RouteId,
                        RouteDirections.Outbound,
                        current.TravelTimeMinutes,
                        route.BaseFare,
                        offset,
                        outWeekday,
                        outWeekend));

                    offset += current.TravelTimeMinutes;
                    byOrigin.TryGetValue(current.DestinationStopId, out current);
                }

                if (visited.Count < segmentCount)
                    logger.LogWarning(
                        "Route {RouteId}: only {Used} of {Total} segments were reachable from the start; the rest were dropped.",
                        routeId, visited.Count, segmentCount);

                // Generate the reverse direction. Offsets restart at 0 from the far terminus,
                // so the last stop of the forward chain becomes the first stop of the return trip.
                // Assumes travel time is the same in both directions.
                if (route.IsBidirectional && (inbound.Weekday is not null || inbound.Weekend is not null))
                {
                    int reverseOffset = 0;

                    for (int i = chain.Count - 1; i >= 0; i--)
                    {
                        var seg = chain[i];

                        edges.Add(new GraphEdge(
                            seg.DestinationStopId,
                            seg.OriginStopId,
                            seg.RouteId,
                            RouteDirections.Inbound,
                            seg.TravelTimeMinutes,
                            route.BaseFare,
                            reverseOffset,
                            inbound.Weekday,
                            inbound.Weekend));

                        reverseOffset += seg.TravelTimeMinutes;
                    }
                }
            }

            var hubIds = context.Stops
                .AsNoTracking()
                .Where(s => s.IsTransferHub && s.IsActive)
                .Select(s => s.Id)
                .ToList();

            return (edges, hubIds);
        }
    }

}
