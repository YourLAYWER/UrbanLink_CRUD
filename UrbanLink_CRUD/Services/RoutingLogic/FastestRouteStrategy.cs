using static UrbanLink_CRUD.Services.RoutingLogic.RoutingContex;

namespace UrbanLink_CRUD.Services.RoutingLogic
{
    public class FastestRouteStrategy : IRoutingStrategy
    {
        private const int TransferPenaltyMinutes = 10;

        public RoutingResult ComputeRoute(
            RoutingContext context,
            IReadOnlyDictionary<int, List<GraphEdge>> graph,
            IReadOnlySet<int> transferHubs)
        {
            var bestTimes = new Dictionary<(int stop, int route), int>();
            var pq = new PriorityQueue<SearchState, int>();
            var cameFrom = new Dictionary<(int stop, int route), ((int stop, int route) prevState, GraphEdge edge)>();

            pq.Enqueue(new SearchState(context.StartStopId, -1, 0), 0);
            bestTimes[(context.StartStopId, -1)] = 0;

            (int stop, int route)? finalState = null;

            while (pq.Count > 0)
            {
                pq.TryDequeue(out var currentState, out int currentTime);

                if (currentState.StopId == context.TargetStopId)
                {
                    finalState = (currentState.StopId, currentState.CurrentRouteId);
                    break;
                }

                if (currentTime > bestTimes.GetValueOrDefault((currentState.StopId, currentState.CurrentRouteId), int.MaxValue))
                    continue;

                if (graph.TryGetValue(currentState.StopId, out var outboundEdges))
                {
                    foreach (var edge in outboundEdges)
                    {
                        bool isChangingBuses = currentState.CurrentRouteId != -1 &&
                                               currentState.CurrentRouteId != edge.RouteId;
                        bool isFirstBoarding = currentState.CurrentRouteId == -1;

                        if (isChangingBuses && !transferHubs.Contains(currentState.StopId))
                        {
                            continue;
                        }

                        int waitTime = 0;

                        if (isFirstBoarding || isChangingBuses)
                        {
                            DateTime userTimeAtThisNode = context.UserDepartureTime.AddMinutes(currentTime);
                            waitTime = CalculateWaitTime(userTimeAtThisNode, edge);

                            if (waitTime >= 9999)
                            {
                                continue;
                            }
                        }

                        int costToNextStop = edge.TravelTimeMinutes + waitTime + (isChangingBuses ? TransferPenaltyMinutes : 0);
                        int newTotalTime = currentTime + costToNextStop;

                        var nextStateKey = (edge.DestinationStopId, edge.RouteId);
                        var currentStateKey = (currentState.StopId, currentState.CurrentRouteId);

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
                return new RoutingResult(-1, new List<JourneyLeg>());
            }

            return ReconstructPath(cameFrom, finalState.Value, bestTimes[finalState.Value]);
        }

        private int CalculateWaitTime(DateTime userCurrentTime, GraphEdge edge)
        {
            bool isWeekend = userCurrentTime.DayOfWeek == DayOfWeek.Saturday ||
                             userCurrentTime.DayOfWeek == DayOfWeek.Sunday;
            string expectedDayType = isWeekend ? "Weekends" : "Weekdays";

            var activeSchedule = edge.Schedules.FirstOrDefault(s => s.DaysOfOperation == expectedDayType);

            if (activeSchedule == null)
            {
                return 9999;
            }

            TimeSpan firstArrivalAtStop = activeSchedule.FirstDepartureTime.Add(TimeSpan.FromMinutes(edge.OffsetFromRouteStart));
            TimeSpan lastArrivalAtStop = activeSchedule.LastDepartureTime.Add(TimeSpan.FromMinutes(edge.OffsetFromRouteStart));
            TimeSpan userTimeOfDay = userCurrentTime.TimeOfDay;

            if (userTimeOfDay > lastArrivalAtStop)
            {
                return 9999;
            }

            if (userTimeOfDay <= firstArrivalAtStop)
            {
                return (int)(firstArrivalAtStop - userTimeOfDay).TotalMinutes;
            }

            int minutesSinceFirstBus = (int)(userTimeOfDay - firstArrivalAtStop).TotalMinutes;
            int minutesIntoCurrentCycle = minutesSinceFirstBus % activeSchedule.FrequencyMinutes;

            if (minutesIntoCurrentCycle == 0) return 0;

            return activeSchedule.FrequencyMinutes - minutesIntoCurrentCycle;
        }

        private RoutingResult ReconstructPath(
            Dictionary<(int stop, int route), ((int stop, int route) prevState, GraphEdge edge)> cameFrom,
            (int stop, int route) targetState,
            int totalTime)
        {
            var pathEdges = new List<GraphEdge>();
            var currentState = targetState;

            while (cameFrom.ContainsKey(currentState))
            {
                var transitData = cameFrom[currentState];
                pathEdges.Add(transitData.edge);
                currentState = transitData.prevState;
            }

            pathEdges.Reverse();

            var journeyLegs = new List<JourneyLeg>();
            if (pathEdges.Count == 0) return new RoutingResult(totalTime, journeyLegs);

            var currentLeg = new JourneyLeg
            {
                RouteId = pathEdges[0].RouteId,
                StartStopId = pathEdges[0].OriginStopId,
                EndStopId = pathEdges[0].DestinationStopId,
                LegDurationMinutes = pathEdges[0].TravelTimeMinutes,
                LegFare = pathEdges[0].RouteBaseFare
            };

            for (int i = 1; i < pathEdges.Count; i++)
            {
                var edge = pathEdges[i];

                if (edge.RouteId == currentLeg.RouteId)
                {
                    currentLeg.EndStopId = edge.DestinationStopId;
                    currentLeg.LegDurationMinutes += edge.TravelTimeMinutes;
                }
                else
                {
                    journeyLegs.Add(currentLeg);
                    currentLeg = new JourneyLeg
                    {
                        RouteId = edge.RouteId,
                        StartStopId = edge.OriginStopId,
                        EndStopId = edge.DestinationStopId,
                        LegDurationMinutes = edge.TravelTimeMinutes,
                        LegFare = edge.RouteBaseFare
                    };
                }
            }

            journeyLegs.Add(currentLeg);

            return new RoutingResult(totalTime, journeyLegs);
        }
    }
}
