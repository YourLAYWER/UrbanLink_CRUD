namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class UserJourney : Journey
    {
        public int UserID { get; set; }
        public int StartStopID { get; set; }
        public DateTime EstimatedArrivalTime { get; set; }

        public virtual User User { get; set; }
    }
}
