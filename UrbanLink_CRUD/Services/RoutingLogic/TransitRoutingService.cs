using static UrbanLink_CRUD.Services.RoutingLogic.RoutingModels;

namespace UrbanLink_CRUD.Services.RoutingLogic
{

    public class TransitRoutingService : ITransitRoutingService
    {
        // The graph: OriginStopId -> List of outbound edges (lightweight records)
        private Dictionary<int, List<GraphEdge>> _graph = new();

        // Stores the IDs of stops where transfers are legally allowed (e.g., the depots)
        private HashSet<int> _transferHubs = new();

        // Configurable time penalty for making a transfer
        private const int TransferPenaltyMinutes = 10;

        /// <summary>
        /// Loads the master routing data into memory. Call this on application startup or caching layer.
        /// </summary>
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

            // Atomic swaps - completely thread safe
            _graph = newGraph;
            _transferHubs = newHubs;
        }

        /// <summary>
        /// Computes the fastest path and reconstructs the journey legs for database insertion.
        /// </summary>
        // 1. UPDATED SIGNATURE: Added TimeSpan userStartTime
        public RoutingResult ComputeFastestRoute(int startStopId, int targetStopId, TimeSpan userStartTime)
        {
            var bestTimes = new Dictionary<(int stop, int route), int>();
            var pq = new PriorityQueue<SearchState, int>();

            // Tracks (CurrentStop, CurrentRoute) -> (PreviousStop, PreviousRoute, EdgeUsed)
            var cameFrom = new Dictionary<(int stop, int route), ((int stop, int route) prevState, GraphEdge edge)>();

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
                if (_graph.TryGetValue(currentState.StopId, out var outboundEdges))
                {
                    foreach (var edge in outboundEdges)
                    {
                        bool isChangingBuses = currentState.CurrentRouteId != -1 &&
                                               currentState.CurrentRouteId != edge.RouteId;

                        // NEW: Are we stepping onto a bus for the very first time?
                        bool isFirstBoarding = currentState.CurrentRouteId == -1;

                        // STRICT RULE: Only allow transfers at designated hubs
                        // Fail fast: If it requires a bus change but we aren't at a hub, skip immediately.
                        if (isChangingBuses && !_transferHubs.Contains(currentState.StopId))
                        {
                            continue; // Illegal move, abandon this path
                        }

                        // ==========================================
                        // NEW TIME-AWARE SCHEDULING LOGIC
                        // ==========================================
                        int waitTime = 0;

                        // We only wait if we are getting on a NEW bus. (If we are on the bus already, wait is 0)
                        if (isFirstBoarding || isChangingBuses)
                        {
                            // Calculate the exact time of day the commuter arrives at this stop
                            TimeSpan userTimeAtThisNode = userStartTime.Add(TimeSpan.FromMinutes(currentTime));

                            waitTime = CalculateWaitTime(userTimeAtThisNode, edge);

                            // If waitTime is massive (9999), they missed the last bus for the day on this route.
                            if (waitTime >= 9999)
                            {
                                continue; // Abandon this path, the bus isn't running anymore!
                            }
                        }

                        // Add the dynamic waitTime to the cost equation!
                        int costToNextStop = edge.TravelTimeMinutes + waitTime + (isChangingBuses ? TransferPenaltyMinutes : 0);
                        // ==========================================

                        int newTotalTime = currentTime + costToNextStop;
                        var nextStateKey = (edge.DestinationStopId, edge.RouteId);
                        var currentStateKey = (currentState.StopId, currentState.CurrentRouteId);

                        // If this path to the next stop is faster, record it and queue it
                        if (newTotalTime < bestTimes.GetValueOrDefault(nextStateKey, int.MaxValue))
                        {
                            bestTimes[nextStateKey] = newTotalTime;
                            cameFrom[nextStateKey] = (currentStateKey, edge);
                            pq.Enqueue(new SearchState(edge.DestinationStopId, edge.RouteId, newTotalTime), newTotalTime);
                        }
                    }
                }
            }

            if (finalState == null)
            {
                return new RoutingResult(-1, new List<JourneyLeg>()); // Indicate no route found
            }

            return ReconstructPath(cameFrom, finalState.Value, bestTimes[finalState.Value]);
        }

        // 2. HELPER METHOD: Add this directly below ComputeFastestRoute
        private int CalculateWaitTime(TimeSpan userCurrentTime, GraphEdge edge)
        {
            // 1. Calculate when the first bus of the day actually reaches THIS specific stop
            TimeSpan firstArrivalAtStop = edge.FirstDepartureTime.Add(TimeSpan.FromMinutes(edge.OffsetFromRouteStart));

            // 2. Calculate when the last bus reaches this stop
            TimeSpan lastArrivalAtStop = edge.LastDepartureTime.Add(TimeSpan.FromMinutes(edge.OffsetFromRouteStart));

            // 3. Did they miss the last bus? (Return a massive penalty to block this route)
            if (userCurrentTime > lastArrivalAtStop)
            {
                return 9999;
            }

            // 4. Are they early for the first bus? 
            if (userCurrentTime <= firstArrivalAtStop)
            {
                return (int)(firstArrivalAtStop - userCurrentTime).TotalMinutes;
            }

            // 5. The bus is currently running. Calculate the wait for the NEXT interval.
            int minutesSinceFirstBus = (int)(userCurrentTime - firstArrivalAtStop).TotalMinutes;
            int minutesIntoCurrentCycle = minutesSinceFirstBus % edge.FrequencyMinutes;

            if (minutesIntoCurrentCycle == 0) return 0; // Bus is boarding right now!

            return edge.FrequencyMinutes - minutesIntoCurrentCycle;
        }

        /// <summary>
        /// Backtracks through the cameFrom dictionary to build the continuous journey legs.
        /// </summary>
        private RoutingResult ReconstructPath(
            Dictionary<(int stop, int route), ((int stop, int route) prevState, GraphEdge edge)> cameFrom,
            (int stop, int route) targetState,
            int totalTime)
        {
            var pathEdges = new List<GraphEdge>();
            var currentState = targetState;

            // Backtrack from destination to origin
            while (cameFrom.ContainsKey(currentState))
            {
                var transitData = cameFrom[currentState];
                pathEdges.Add(transitData.edge);
                currentState = transitData.prevState;
            }

            // Reverse to get chronological order (Start -> Target)
            pathEdges.Reverse();

            var journeyLegs = new List<JourneyLeg>();
            if (pathEdges.Count == 0) return new RoutingResult(totalTime, journeyLegs);

            var currentLeg = new JourneyLeg
            {
                RouteId = pathEdges[0].RouteId,
                StartStopId = pathEdges[0].OriginStopId,
                EndStopId = pathEdges[0].DestinationStopId,
                LegDurationMinutes = pathEdges[0].TravelTimeMinutes,

                // Apply the flat fare once when they board this route
                LegFare = pathEdges[0].RouteBaseFare
            };

            for (int i = 1; i < pathEdges.Count; i++)
            {
                var edge = pathEdges[i];

                if (edge.RouteId == currentLeg.RouteId)
                {
                    // STAYING ON THE SAME BUS
                    currentLeg.EndStopId = edge.DestinationStopId;
                    currentLeg.LegDurationMinutes += edge.TravelTimeMinutes;

                    // Notice we DO NOT add to LegFare here anymore!
                }
                else
                {
                    // CHANGING BUSES
                    journeyLegs.Add(currentLeg);
                    currentLeg = new JourneyLeg
                    {
                        RouteId = edge.RouteId,
                        StartStopId = edge.OriginStopId,
                        EndStopId = edge.DestinationStopId,
                        LegDurationMinutes = edge.TravelTimeMinutes,

                        // Apply the new flat fare for boarding the new route
                        LegFare = edge.RouteBaseFare
                    };
                }
            }

            journeyLegs.Add(currentLeg);

            return new RoutingResult(totalTime, journeyLegs);
        }
    }
}
