using UrbanLink_CRUD.Models.Entities.Transit;
using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Services.RoutingLogic
{
    public class FastestRouteStrategy : IRoutingStrategy
    {
        // Time needed to walk/change between vehicles. A transfer rider cannot board
        // anything departing sooner than this, so it is applied BEFORE computing
        // the wait (not added on top of it).
        private const int TransferBufferMinutes = 10;
        private const int NotBoarded = -1;

        // Line = GraphEdge.LineKey (route + direction).
        private readonly record struct StateKey(int Stop, int Line);

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
                bool boarding = cur.Line == NotBoarded;
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
                    // Changing line includes reversing direction on the same route.
                    bool changing = !boarding && edge.LineKey != cur.Line;
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

                    var nextKey = new StateKey(edge.DestinationStopId, edge.LineKey);

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

            // Each edge paired with the time (minutes after the search time) at which it ends.
            var path = new List<(GraphEdge Edge, int ArrivalTime)>();
            while (node.Edge is not null)
            {
                path.Add((node.Edge, node.Time));
                node = nodes[node.Prev];
            }
            path.Reverse();

            var legs = new List<JourneyLeg>();
            if (path.Count == 0) return new RoutingResult(totalTime, legs);

            var current = NewLeg(path[0].Edge, path[0].ArrivalTime);
            int currentLine = path[0].Edge.LineKey;

            for (int i = 1; i < path.Count; i++)
            {
                var (edge, arrival) = path[i];

                // Compare lines, not route ids, so an out-and-back isn't merged into one leg.
                if (edge.LineKey == currentLine)
                {
                    current.EndStopId = edge.DestinationStopId;
                    current.LegDurationMinutes += edge.TravelTimeMinutes;
                    current.ArrivalOffsetMinutes = arrival;
                }
                else
                {
                    legs.Add(current);
                    current = NewLeg(edge, arrival);
                    currentLine = edge.LineKey;
                }
            }

            legs.Add(current);
            return new RoutingResult(totalTime, legs);
        }

        private static JourneyLeg NewLeg(GraphEdge edge, int arrivalTime) => new()
        {
            RouteId = edge.RouteId,
            Direction = edge.Direction,
            StartStopId = edge.OriginStopId,
            EndStopId = edge.DestinationStopId,
            LegDurationMinutes = edge.TravelTimeMinutes,
            LegFare = edge.RouteBaseFare,
            // The vehicle leaves the origin exactly TravelTime before it reaches the destination,
            // so boarding time = arrival - travel (this already includes buffer and wait).
            DepartureOffsetMinutes = arrivalTime - edge.TravelTimeMinutes,
            ArrivalOffsetMinutes = arrivalTime
        };


    }
}
