using UrbanLink_CRUD.Models.Entities.Transit;

namespace UrbanLink_CRUD.Services.RoutingLogic
{
    public class FastestRouteStrategy : IRoutingStrategy
    {
        // Time needed to walk/change between buses. A transfer rider cannot board
        // anything departing sooner than this, so it is applied BEFORE computing
        // the wait (not added on top of it).
        private const int TransferBufferMinutes = 10;
        private const int NotBoarded = -1;

        private readonly record struct StateKey(int Stop, int Route);

        // One dictionary instead of bestTimes + cameFrom: half the hashing.
        private readonly record struct Node(int Time, StateKey Prev, GraphEdge? Edge);

        public RoutingResult ComputeRoute(
            RoutingContext context,
            IReadOnlyDictionary<int, List<GraphEdge>> graph,
            IReadOnlySet<int> transferHubs)
        {
            var nodes = new Dictionary<StateKey, Node>();
            var pq = new PriorityQueue<StateKey, int>();

            var start = new StateKey(context.StartStopId, NotBoarded);
            nodes[start] = new Node(0, default, null);
            pq.Enqueue(start, 0);

            StateKey? finalState = null;

            while (pq.TryDequeue(out var cur, out int time))
            {
                if (time > nodes[cur].Time) continue;          // stale queue entry

                if (cur.Stop == context.TargetStopId)
                {
                    finalState = cur;
                    break;
                }

                if (!graph.TryGetValue(cur.Stop, out var edges)) continue;

                // Per-state invariants: computed once, not once per edge.
                bool boarding = cur.Route == NotBoarded;
                bool atHub = transferHubs.Contains(cur.Stop);

                int minuteOfDay = 0;
                DayType day = DayType.Weekday;

                if (boarding || atHub)
                {
                    int buffer = boarding ? 0 : TransferBufferMinutes;
                    var readyToBoard = context.UserDepartureTime.AddMinutes(time + buffer);

                    // Ceiling so 08:00:40 can't catch the 08:00 bus that already left.
                    minuteOfDay = (int)Math.Ceiling(readyToBoard.TimeOfDay.TotalMinutes);
                    day = readyToBoard.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
                        ? DayType.Weekend
                        : DayType.Weekday;
                }

                foreach (var edge in edges)
                {
                    bool changing = !boarding && edge.RouteId != cur.Route;
                    if (changing && !atHub) continue;

                    int wait = 0;
                    if (boarding || changing)
                    {
                        var schedule = edge.ScheduleFor(day);
                        if (schedule is null) continue;

                        wait = schedule.Value.WaitFrom(minuteOfDay, edge.OffsetFromRouteStart);
                        if (wait == ScheduleInfo.NoService) continue;
                    }

                    int next = time
                             + (changing ? TransferBufferMinutes : 0)
                             + wait
                             + edge.TravelTimeMinutes;

                    var nextKey = new StateKey(edge.DestinationStopId, edge.RouteId);

                    if (!nodes.TryGetValue(nextKey, out var existing) || next < existing.Time)
                    {
                        nodes[nextKey] = new Node(next, cur, edge);
                        pq.Enqueue(nextKey, next);
                    }
                }
            }

            if (finalState is null)
                return new RoutingResult(-1, new List<JourneyLeg>());

            return ReconstructPath(nodes, finalState.Value);
        }

        private static RoutingResult ReconstructPath(Dictionary<StateKey, Node> nodes, StateKey target)
        {
            var node = nodes[target];
            int totalTime = node.Time;

            var pathEdges = new List<GraphEdge>();
            while (node.Edge is not null)
            {
                pathEdges.Add(node.Edge);
                node = nodes[node.Prev];
            }
            pathEdges.Reverse();

            var legs = new List<JourneyLeg>();
            if (pathEdges.Count == 0) return new RoutingResult(totalTime, legs);

            var current = NewLeg(pathEdges[0]);

            for (int i = 1; i < pathEdges.Count; i++)
            {
                var edge = pathEdges[i];

                if (edge.RouteId == current.RouteId)
                {
                    current.EndStopId = edge.DestinationStopId;
                    current.LegDurationMinutes += edge.TravelTimeMinutes;
                }
                else
                {
                    legs.Add(current);
                    current = NewLeg(edge);
                }
            }

            legs.Add(current);
            return new RoutingResult(totalTime, legs);
        }

        private static JourneyLeg NewLeg(GraphEdge edge) => new()
        {
            RouteId = edge.RouteId,
            StartStopId = edge.OriginStopId,
            EndStopId = edge.DestinationStopId,
            LegDurationMinutes = edge.TravelTimeMinutes,
            LegFare = edge.RouteBaseFare
        };

    }
}
