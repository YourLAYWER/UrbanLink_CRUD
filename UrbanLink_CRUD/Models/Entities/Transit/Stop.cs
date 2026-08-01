namespace UrbanLink_CRUD.Models.Entities.Transit
{
    public class Stop
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;

        //Using double for precision in latitude and longitude
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
