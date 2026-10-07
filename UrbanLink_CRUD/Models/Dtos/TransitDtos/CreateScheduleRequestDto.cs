namespace UrbanLink_CRUD.Models.Dtos.Transit
{
    public class CreateScheduleRequestDto
    {
        public int RouteId { get; set; }
        public string DaysOfOperation { get; set; } = string.Empty;

        // "HH:mm", e.g. "06:00". Parsed server-side — TimeSpan has no built-in
        // System.Text.Json support for this format.
        public string FirstDepartureTime { get; set; } = string.Empty;
        public string LastDepartureTime { get; set; } = string.Empty;
        public int FrequencyMinutes { get; set; }
    }
}
