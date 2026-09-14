using Microsoft.EntityFrameworkCore;
using UrbanLink_CRUD.Models.Entities;
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
        public DbSet<Entities.Transit.RouteSegment> RouteSegments { get; set; } = null!;
        public DbSet<Entities.Transit.TransitProvider> TransitProviders { get; set; } = null!;
        public DbSet<Entities.Transit.TransitRoute> TransitRoutes { get; set; } = null!;
        public DbSet<Entities.Transit.RouteStop> RouteStops { get; set; } = null!;
        public DbSet<Entities.Transit.JourneyLeg> JourneyLegs { get; set; } = null!;

        // NEW: Journey TPT DbSets
        public DbSet<Entities.Transit.Journey> Journeys { get; set; } = null!;
        public DbSet<Entities.Transit.UserJourney> UserJourneys { get; set; } = null!;
        public DbSet<Entities.Transit.CashJourney> CashJourneys { get; set; } = null!;

        public DbSet<Bus> Buses { get; set; }
        public DbSet<ConductorShift> ConductorShifts { get; set; }
        public DbSet<RouteSchedule> RouteSchedules { get; set; } = null!;

        // Incident Reporting
        public DbSet<IncidentReport> IncidentReports { get; set; } = null!;

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

            // Enum Conversions
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<Bus>()
                .Property(b => b.Status)
                .HasConversion<string>();

            // NEW: JourneyStatus Enum Conversion
            modelBuilder.Entity<Journey>()
                .Property(j => j.Status)
                .HasConversion<string>();

            // ----------------------------------------------------
            // NEW: JOURNEY TABLE-PER-TYPE (TPT) CONFIGURATION
            // ----------------------------------------------------
            modelBuilder.Entity<Journey>().ToTable("Journeys");
            modelBuilder.Entity<UserJourney>().ToTable("UserJourneys");
            modelBuilder.Entity<CashJourney>().ToTable("CashJourneys");

            // Precision for Base Journey Cost
            modelBuilder.Entity<Journey>()
                .Property(j => j.TotalCost)
                .HasPrecision(18, 2);

            // UserJourney Relationships
            modelBuilder.Entity<UserJourney>()
                .HasOne(u => u.User)
                .WithMany()
                .HasForeignKey(u => u.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Map StartStop to UserJourney (Moved from base Journey)
            modelBuilder.Entity<UserJourney>()
                .HasOne<Stop>()
                .WithMany()
                .HasForeignKey(u => u.StartStopID)
                .OnDelete(DeleteBehavior.Restrict);

            // CashJourney -> Shift Relationship
            modelBuilder.Entity<CashJourney>()
                .HasOne(c => c.Shift)
                .WithMany(cs => cs.CashJourneys)
                .HasForeignKey(c => c.ShiftID)
                .OnDelete(DeleteBehavior.Restrict);

            // Base Journey EndStop mapping (Retained for future route-stop logic)
            modelBuilder.Entity<Journey>()
                .HasOne<Stop>()
                .WithMany()
                .HasForeignKey(j => j.EndStopID)
                .OnDelete(DeleteBehavior.Restrict);


            // ----------------------------------------------------
            // CONDUCTOR Configuration
            // ----------------------------------------------------
            modelBuilder.Entity<Bus>()
                .HasOne(b => b.Provider)
                .WithMany()
                .HasForeignKey(b => b.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ConductorShift>()
                .HasOne(cs => cs.Bus)
                .WithMany()
                .HasForeignKey(cs => cs.BusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ConductorShift>()
                .HasOne(cs => cs.Route)
                .WithMany()
                .HasForeignKey(cs => cs.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ConductorShift>()
                .Property(cs => cs.TotalCashCollected)
                .HasPrecision(18, 2);

            modelBuilder.Entity<IncidentReport>()
                .HasOne(ir => ir.ConductorShift)
                .WithMany(cs => cs.IncidentReports)
                .HasForeignKey(ir => ir.ConductorShiftId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------------------------------------
            // ROUTE SCHEDULE & LEGS Configuration
            // ----------------------------------------------------
            modelBuilder.Entity<TransitRoute>()
                .HasMany(tr => tr.Schedules)
                .WithOne(rs => rs.TransitRoute)
                .HasForeignKey(rs => rs.RouteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JourneyLeg>()
                .HasOne(jl => jl.BoardingStop)
                .WithMany()
                .HasForeignKey(jl => jl.BoardingStopId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JourneyLeg>()
                .HasOne(jl => jl.DestinationStop)
                .WithMany()
                .HasForeignKey(jl => jl.DestinationStopId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------------------------------------
            // ROUTE SEGMENT & STOP Configuration
            // ----------------------------------------------------
            modelBuilder.Entity<RouteSegment>()
                .HasOne(rs => rs.OriginStop)
                .WithMany()
                .HasForeignKey(rs => rs.OriginStopId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RouteSegment>()
                .HasOne(rs => rs.DestinationStop)
                .WithMany()
                .HasForeignKey(rs => rs.DestinationStopId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RouteStop>()
                .HasOne(rs => rs.Stop)
                .WithMany(s => s.RouteStops)
                .HasForeignKey(rs => rs.StopId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RouteStop>()
                .HasOne(rs => rs.RouteSegment)
                .WithMany()
                .HasForeignKey(rs => rs.SegmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransitRoute>()
                .ToTable("TransitRoute");
        }
    }
}