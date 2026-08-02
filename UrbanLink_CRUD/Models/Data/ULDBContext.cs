using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Entities.Aggregator;



namespace UrbanLink_CRUD.Models.Data
{
    public class ULDBContext : DbContext
    {
        public ULDBContext(DbContextOptions<ULDBContext> options) : base(options)
        {
        }

        public DbSet<Entities.User> Users { get; set; } = null!;
        public DbSet<Entities.Transit.Stop> Stops { get; set; } = null!;
        public DbSet<Entities.Transit.Journey> Journeys { get; set; } = null!;
        public DbSet<Entities.Transit.RouteSegment> RouteSegments { get; set; } = null!;
        public DbSet<Entities.Transit.TransitProvider> TransitProviders { get; set; } = null!;
        public DbSet<Retailer> Retailers { get; set; } = null!;
        public DbSet<RetailerProduct> RetailerProducts { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Entities.Notification> Notifications { get; set; } = null!;
    }
}