namespace UrbanLink_CRUD.Services.Routing_Logic
{
    public class TransitSystemRouting
    {
        // --- 1. DATA MODELS ---

        // Represents the edge in our graph fetched from the DB
        public record RouteSegment(int OriginStopId, int DestinationStopId, int RouteId, int TravelTimeMinutes);

        // Represents the state in our Priority Queue
        public record SearchState(int StopId, int CurrentRouteId, int TotalTimeMinutes);

        // Represents the record that will be inserted into your JourneyLeg table
        public class JourneyLeg
        {
            public int RouteId { get; set; }
            public int StartStopId { get; set; }
            public int EndStopId { get; set; }
            public int LegDurationMinutes { get; set; }
        }

        // A wrapper to return both the total time and the legs
        public record RoutingResult(int TotalTimeMinutes, List<JourneyLeg> Legs);


        // --- 2. ROUTING SERVICE ---

        public class TransitRoutingService
        {
            // The graph: OriginStopId -> List of outbound segments
            private  Dictionary<int, List<RouteSegment>> _graph = new();

            // Stores the IDs of stops where transfers are legally allowed (e.g., the 4 depots)
            private  HashSet<int> _transferHubs = new();

            // Configurable time penalty for making a transfer
            private const int TransferPenaltyMinutes = 10;

            /// <summary>
            /// Loads the master routing data into memory. Call this on application startup or caching layer.
            /// </summary>
            public void InitializeGraph(IEnumerable<RouteSegment> dbSegments, IEnumerable<int> transferHubIds)
            {
                var newGraph = new Dictionary<int, List<RouteSegment>>();
                var newHubs = new HashSet<int>(transferHubIds);

                foreach (var segment in dbSegments)
                {
                    if (!newGraph.ContainsKey(segment.OriginStopId))
                        newGraph[segment.OriginStopId] = new List<RouteSegment>();

                    newGraph[segment.OriginStopId].Add(segment);
                }

                // Atomic swaps - completely thread safe
                _graph = newGraph;
                _transferHubs = newHubs;
            }

            /// <summary>
            /// Computes the fastest path and reconstructs the journey legs for database insertion.
            /// </summary>
            public RoutingResult ComputeFastestRoute(int startStopId, int targetStopId)
            {
                var bestTimes = new Dictionary<(int stop, int route), int>();
                var pq = new PriorityQueue<SearchState, int>();

                // Tracks (CurrentStop, CurrentRoute) -> (PreviousStop, PreviousRoute, SegmentUsed)
                var cameFrom = new Dictionary<(int stop, int route), ((int stop, int route) prevState, RouteSegment segment)>();

                // -1 represents the state before boarding any bus
                pq.Enqueue(new SearchState(startStopId, -1, 0), 0);
                bestTimes[(startStopId, -1)] = 0;

                (int stop, int route)? finalState = null;

                while (pq.Count > 0)
                {
                    pq.TryDequeue(out var currentState, out int currentTime);

                    // Reached the target stop
                    if (currentState.StopId == targetStopId)
                    {
                        finalState = (currentState.StopId, currentState.CurrentRouteId);
                        break;
                    }

                    // Discard stale states if we've already found a faster path
                    if (currentTime > bestTimes.GetValueOrDefault((currentState.StopId, currentState.CurrentRouteId), int.MaxValue))
                        continue;

                    // Evaluate all outbound segments from the current stop
                    if (_graph.TryGetValue(currentState.StopId, out var outboundSegments))
                    {
                        foreach (var segment in outboundSegments)
                        {
                            //int costToNextStop = segment.TravelTimeMinutes;

                            bool isChangingBuses = currentState.CurrentRouteId != -1 &&
                                                   currentState.CurrentRouteId != segment.RouteId;

                            // STRICT RULE: Only allow transfers at designated hubs
                            // Fail fast: If it requires a bus change but we aren't at a hub, skip immediately.
                            if (isChangingBuses && !_transferHubs.Contains(currentState.StopId))
                            {
                                continue; // Illegal move, abandon this path
                            }
                            int costToNextStop = segment.TravelTimeMinutes + (isChangingBuses ? TransferPenaltyMinutes : 0);

                            int newTotalTime = currentTime + costToNextStop;
                            var nextStateKey = (segment.DestinationStopId, segment.RouteId);
                            var currentStateKey = (currentState.StopId, currentState.CurrentRouteId);

                            // If this path to the next stop is faster, record it and queue it
                            if (newTotalTime < bestTimes.GetValueOrDefault(nextStateKey, int.MaxValue))
                            {
                                bestTimes[nextStateKey] = newTotalTime;
                                cameFrom[nextStateKey] = (currentStateKey, segment);
                                pq.Enqueue(new SearchState(segment.DestinationStopId, segment.RouteId, newTotalTime), newTotalTime);
                            }
                        }
                    }
                }

                if (finalState == null)
                {
                    // throw new Exception($"No valid route found between stop {startStopId} and {targetStopId}.");
                    return new RoutingResult(-1, new List<JourneyLeg>()); // Indicate no route found
                }

                return ReconstructPath(cameFrom, finalState.Value, bestTimes[finalState.Value]);
            }

            /// <summary>
            /// Backtracks through the cameFrom dictionary to build the continuous journey legs.
            /// </summary>
            private RoutingResult ReconstructPath(
                Dictionary<(int stop, int route), ((int stop, int route) prevState, RouteSegment segment)> cameFrom,
                (int stop, int route) targetState,
                int totalTime)
            {
                var pathSegments = new List<RouteSegment>();
                var currentState = targetState;

                // Backtrack from destination to origin
                while (cameFrom.ContainsKey(currentState))
                {
                    var transitData = cameFrom[currentState];
                    pathSegments.Add(transitData.segment);
                    currentState = transitData.prevState;
                }

                // Reverse to get chronological order (Start -> Target)
                pathSegments.Reverse();

                var journeyLegs = new List<JourneyLeg>();
                if (pathSegments.Count == 0) return new RoutingResult(totalTime, journeyLegs);

                var currentLeg = new JourneyLeg
                {
                    RouteId = pathSegments[0].RouteId,
                    StartStopId = pathSegments[0].OriginStopId,
                    EndStopId = pathSegments[0].DestinationStopId,
                    LegDurationMinutes = pathSegments[0].TravelTimeMinutes
                };

                for (int i = 1; i < pathSegments.Count; i++)
                {
                    var segment = pathSegments[i];

                    if (segment.RouteId == currentLeg.RouteId)
                    {
                        // Extend the current leg along the same route
                        currentLeg.EndStopId = segment.DestinationStopId;
                        currentLeg.LegDurationMinutes += segment.TravelTimeMinutes;
                    }
                    else
                    {
                        // Bus change occurred: save current leg and start a new one
                        journeyLegs.Add(currentLeg);
                        currentLeg = new JourneyLeg
                        {
                            RouteId = segment.RouteId,
                            StartStopId = segment.OriginStopId,
                            EndStopId = segment.DestinationStopId,
                            LegDurationMinutes = segment.TravelTimeMinutes
                        };
                    }
                }

                journeyLegs.Add(currentLeg);

                return new RoutingResult(totalTime, journeyLegs);
            }
        }
    }
}
