namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class CashJourney : Journey
    {
        public int ShiftID { get; set; }

        public virtual ConductorShift Shift { get; set; }
    }
}
