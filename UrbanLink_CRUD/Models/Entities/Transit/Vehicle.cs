namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public string MakeModel { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int ProviderId { get; set; }

        // Navigation property
        public virtual TransitProvider? Provider { get; set; }
    }
}