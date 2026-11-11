using Microsoft.EntityFrameworkCore;


namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class CardTopUp
    {
        public int Id { get; set; }
        public int ConductorShiftId { get; set; }
        public virtual ConductorShift Shift { get; set; }

        public int ProviderCardId { get; set; }
        public virtual ProviderCard Card { get; set; }

        public decimal AmountLoaded { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}