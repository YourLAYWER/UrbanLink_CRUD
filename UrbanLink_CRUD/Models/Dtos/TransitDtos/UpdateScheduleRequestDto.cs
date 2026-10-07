namespace UrbanLink_CRUD.Models.Dtos.Transit
{
    public class UpdateScheduleRequestDto
    {
        public string DaysOfOperation { get; set; } = string.Empty;
        public string FirstDepartureTime { get; set; } = string.Empty;
        public string LastDepartureTime { get; set; } = string.Empty;
        public int FrequencyMinutes { get; set; }
    }
}
