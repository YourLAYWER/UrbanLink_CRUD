using UrbanLink_CRUD.Models.Entities.Transit;

namespace UrbanLink_CRUD.Services.RoutingLogic
{
    public enum DayType { Weekday, Weekend }

    /// <summary>
    /// Lightweight, precomputed version of RouteSchedule. Built once when the graph is
    /// built, so the search loop never touches TimeSpan, strings or LINQ.
    /// </summary>
    public readonly record struct ScheduleInfo(int FirstDepartureMin, int LastDepartureMin, int FrequencyMinutes)
    {
        public const int NoService = int.MaxValue;

        public static ScheduleInfo From(RouteSchedule s)
        {
            if (s.FrequencyMinutes <= 0)
                throw new InvalidOperationException(
                    $"RouteSchedule {s.ScheduleId} has FrequencyMinutes <= 0.");

            return new ScheduleInfo(
                (int)s.FirstDepartureTime.TotalMinutes,
                (int)s.LastDepartureTime.TotalMinutes,
                s.FrequencyMinutes);
        }

        /// <summary>
        /// Resolves a route's schedules into one weekday and one weekend schedule.
        /// Call once per route when building the graph, not once per edge.
        /// "Daily" is treated as both; anything else (e.g. "Holiday") is ignored,
        /// same as before. If a route has several rows for the same day type,
        /// the first one wins (same as the old FirstOrDefault).
        /// </summary>
        public static (ScheduleInfo? Weekday, ScheduleInfo? Weekend) Resolve(IEnumerable<RouteSchedule> schedules)
        {
            ScheduleInfo? weekday = null, weekend = null;

            foreach (var s in schedules)
            {
                var days = s.DaysOfOperation.Trim();
                bool daily = days.Equals("Daily", StringComparison.OrdinalIgnoreCase);

                if (daily || days.Equals("Weekdays", StringComparison.OrdinalIgnoreCase))
                    weekday ??= From(s);

                if (daily || days.Equals("Weekends", StringComparison.OrdinalIgnoreCase))
                    weekend ??= From(s);
            }

            return (weekday, weekend);
        }

        /// <summary>
        /// Minutes until the next departure at a stop, given the minute-of-day the
        /// rider is ready to board. Returns NoService if no departure is left today.
        /// </summary>
        public int WaitFrom(int minuteOfDay, int offsetFromRouteStart)
        {
            int first = FirstDepartureMin + offsetFromRouteStart;
            int last = LastDepartureMin + offsetFromRouteStart;

            if (minuteOfDay > last) return NoService;
            if (minuteOfDay <= first) return first - minuteOfDay;

            int intoCycle = (minuteOfDay - first) % FrequencyMinutes;
            int wait = intoCycle == 0 ? 0 : FrequencyMinutes - intoCycle;

            // The next computed departure may fall after the last real one
            // (e.g. last = 22:10 with a 15 min frequency).
            return minuteOfDay + wait > last ? NoService : wait;
        }
    }

}
