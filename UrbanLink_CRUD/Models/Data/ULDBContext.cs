using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Entities.Aggregator;
using UrbanLink_CRUD.Models.Entities.Transit;



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
        public DbSet<Entities.Transit.TransitRoute> TransitRoutes { get; set; } = null!;

        public DbSet<Entities.Transit.RouteStop> RouteStops { get; set; } = null!;
        public DbSet<Entities.Transit.JourneyLeg> JourneyLegs { get; set; } = null!;

        // NEW: Tell EF Core to track the RouteSchedules table
        public DbSet<RouteSchedule> RouteSchedules { get; set; } = null!;

        public DbSet<Retailer> Retailers { get; set; } = null!;
        public DbSet<RetailerProduct> RetailerProducts { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Entities.Notification> Notifications { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;

        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ----------------------------------------------------
            // NEW: ROUTE SCHEDULE Configuration
            // ----------------------------------------------------
            modelBuilder.Entity<Entities.Transit.TransitRoute>()
                .HasMany(tr => tr.Schedules)
                .WithOne(rs => rs.TransitRoute)
                .HasForeignKey(rs => rs.RouteId)
                .OnDelete(DeleteBehavior.Cascade); // Automatically clean up schedules if a route is deleted

            // Disable cascade delete for Boarding Stop to prevent multiple cascade paths
            modelBuilder.Entity<Entities.Transit.JourneyLeg>()
                .HasOne(jl => jl.BoardingStop)
                .WithMany()
                .HasForeignKey(jl => jl.BoardingStopId)
                .OnDelete(DeleteBehavior.Restrict);

            // Disable cascade delete for Destination Stop to prevent multiple cascade paths
            modelBuilder.Entity<Entities.Transit.JourneyLeg>()
                .HasOne(jl => jl.DestinationStop)
                .WithMany()
                .HasForeignKey(jl => jl.DestinationStopId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------------------------------------
            // JOURNEY: Disable cascade deletes
            // ----------------------------------------------------
            modelBuilder.Entity<Entities.Transit.Journey>()
                .HasOne(j => j.StartStop)
                .WithMany()
                .HasForeignKey(j => j.StartStopId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Entities.Transit.Journey>()
                .HasOne(j => j.EndStop)
                .WithMany()
                .HasForeignKey(j => j.EndStopId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------------------------------------
            // ROUTE SEGMENT: Disable cascade deletes
            // ----------------------------------------------------
            modelBuilder.Entity<Entities.Transit.RouteSegment>()
                .HasOne(rs => rs.OriginStop)
                .WithMany()
                .HasForeignKey(rs => rs.OriginStopId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Entities.Transit.RouteSegment>()
                .HasOne(rs => rs.DestinationStop)
                .WithMany()
                .HasForeignKey(rs => rs.DestinationStopId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------------------------------------
            // ROUTE STOP: Disable cascade deletes to prevent cycles
            // ----------------------------------------------------
            modelBuilder.Entity<Entities.Transit.RouteStop>()
                .HasOne(rs => rs.Stop)
                .WithMany(s => s.RouteStops)
                .HasForeignKey(rs => rs.StopId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Entities.Transit.RouteStop>()
                .HasOne(rs => rs.RouteSegment)
                .WithMany()
                .HasForeignKey(rs => rs.SegmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Entities.Transit.TransitRoute>()
                .ToTable("TransitRoute");
        }
    }
}