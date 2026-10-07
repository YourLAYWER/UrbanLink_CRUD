using Microsoft.Extensions.Configuration.UserSecrets;
using UrbanLink_CRUD.Models.Enums;

namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class Journey
    {
        public int JourneyID { get; set; }
        public JourneyStatus Status { get; set; } = JourneyStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid TicketReference { get; set; } = Guid.NewGuid();
        public DateTime? RedeemedAt { get; set; }
        public int EndStopID { get; set; }
        public decimal TotalCost { get; set; }

        public virtual ICollection<JourneyLeg> Legs { get; set; } = new List<JourneyLeg>();
    }
}
