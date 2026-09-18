using BCrypt.Net;
using UrbanLink_CRUD.Models.Entities;
using UrbanLink_CRUD.Models.Entities.Transit;
using UrbanLink_CRUD.Models.Enums;


namespace UrbanLink_CRUD.Models.Data
{
    public class DbInitializer
    {
        public static void SeedAdminUser(ULDBContext context)
        {
            if (!context.Users.Any(u => u.Email == "admin@urbanlink.co.za"))
            {
                string adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("AdminSecure@2026!", 11);
                

                var adminUser = new User
                {
                    Name = "System Administrator",
                    Email = "admin@urbanlink.co.za",
                    Password = adminPasswordHash,
                    Role = Models.Enums.Role.Admin,
                    CreatedAt = DateTime.UtcNow
                };

                

                context.Users.Add(adminUser);
                context.SaveChanges();
            }

            if(!context.Users.Any(u => u.Email == "conductor@urbanlink.co.za"))
            {
                string conductorPasswordHash = BCrypt.Net.BCrypt.HashPassword("ConductorSecure@2026!", 11);

                var conductorUser = new User
                {
                    Name = "Yongama Makhala",
                    Email = "conductor@urbanlink.co.za",
                    Password = conductorPasswordHash,
                    Role = Models.Enums.Role.Conductor,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(conductorUser);
                context.SaveChanges();
            }

            if (!context.Users.Any(u => u.Email == "driver@urbanlink.co.za"))
            {
                string conductorPasswordHash = BCrypt.Net.BCrypt.HashPassword("DriverSecure@2026!", 11);

                var driverUser = new User
                {
                    Name = "Gcina Makhala",
                    Email = "driver@urbanlink.co.za",
                    Password = conductorPasswordHash,
                    Role = Models.Enums.Role.Driver,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(driverUser);
                context.SaveChanges();
            }
        }

        public static void SeedTransitProviders(ULDBContext context)
        {
            if (!context.TransitProviders.Any())
            {
                context.TransitProviders.AddRange(
                    new TransitProvider { Name = "Algoa Bus", Type = TransitType.Bus, APIEndpoint = "" },
                    new TransitProvider { Name = "PRASA", Type = TransitType.Train, APIEndpoint = "" },
                    new TransitProvider { Name = "Gqeberha Municipal Bus", Type = TransitType.Bus, APIEndpoint = "" }
                );
                context.SaveChanges();
            }
        }

        public static void SeedBuses(ULDBContext context)
        {
            if (!context.Buses.Any())
            {
                // Get the Algoa Bus provider ID so we can assign the buses to them
                var provider = context.TransitProviders.FirstOrDefault(p => p.Name == "Algoa Bus");

                if (provider != null)
                {
                    context.Buses.AddRange(
                        new Bus
                        {
                            FleetNumber = "ALGOA-042",
                            LicensePlate = "CAA 12345",
                            SeatingCapacity = 60,
                            StandingCapacity = 15,
                            Status = BusStatus.Active,
                            ProviderId = provider.Id
                        },
                        new Bus
                        {
                            FleetNumber = "ALGOA-088",
                            LicensePlate = "CAA 67890",
                            SeatingCapacity = 60,
                            StandingCapacity = 15,
                            Status = BusStatus.Active,
                            ProviderId = provider.Id
                        },
                        new Bus
                        {
                            FleetNumber = "ALGOA-099",
                            LicensePlate = "CAA 11223",
                            SeatingCapacity = 45,
                            StandingCapacity = 10,
                            Status = BusStatus.InMaintenance, // This should NOT show up in the Android app dropdown
                            ProviderId = provider.Id
                        }
                    );
                    context.SaveChanges();
                }
            }
        }

        public static void SeedTransitSystem(ULDBContext context)
        {
            if (context.Stops.Any())
            {
                return;
            }

            var stops = new Stop[]
            {
        new Stop { Name = "Greenacres Shopping Centre", Latitude = -33.9450, Longitude = 25.5720, IsTransferHub = true, IsActive = true },
        new Stop { Name = "SABS", Latitude = -33.9500, Longitude = 25.5900, IsTransferHub = false, IsActive = true },
        new Stop { Name = "BP", Latitude = -33.9550, Longitude = 25.6000, IsTransferHub = false, IsActive = true },
        new Stop { Name = "Gqeberha Central Station", Latitude = -33.9608, Longitude = 25.6222, IsTransferHub = true, IsActive = true },
        new Stop { Name = "KingsBeach", Latitude = -33.9750, Longitude = 25.6450, IsTransferHub = false, IsActive = true },
        new Stop { Name = "Boardwalk", Latitude = -33.9825, Longitude = 25.6580, IsTransferHub = false, IsActive = true },
        new Stop { Name = "NMU South Campus", Latitude = -34.0084, Longitude = 25.6698, IsTransferHub = false, IsActive = true }
            };

            context.Stops.AddRange(stops);
            context.SaveChanges();

            // ==========================================
            // NEW: Fetch Algoa Bus instead of creating a new provider
            // ==========================================
            var provider = context.TransitProviders.FirstOrDefault(p => p.Name == "Algoa Bus");

            // Safety check just in case the providers haven't seeded yet
            if (provider == null)
            {
                provider = new TransitProvider { Name = "Algoa Bus", Type = TransitType.Bus, APIEndpoint = "" };
                context.TransitProviders.Add(provider);
                context.SaveChanges();
            }

            // ==========================================
            // 4. UPDATED: Transit Routes with RouteSchedules
            // ==========================================
            var routes = new TransitRoute[]
            {
        new TransitRoute
        {
            RouteName = "Route 1: Greenacres to Central",
            TransportType = TransitType.Bus,
            BaseFare = 15.00m,
            ProviderId = provider.Id, // Links directly to Algoa Bus!
            Schedules = new List<RouteSchedule>
            {
                new RouteSchedule
                {
                    DaysOfOperation = "Weekdays",
                    FirstDepartureTime = new TimeSpan(6, 0, 0),
                    LastDepartureTime = new TimeSpan(20, 0, 0),
                    FrequencyMinutes = 30
                },
                new RouteSchedule
                {
                    DaysOfOperation = "Weekends",
                    FirstDepartureTime = new TimeSpan(8, 0, 0),
                    LastDepartureTime = new TimeSpan(18, 0, 0),
                    FrequencyMinutes = 60
                }
            }
        },
        new TransitRoute
        {
            RouteName = "Route 2: Central to NMU South",
            TransportType = TransitType.Bus,
            BaseFare = 15.00m,
            ProviderId = provider.Id, // Links directly to Algoa Bus!
            Schedules = new List<RouteSchedule>
            {
                new RouteSchedule
                {
                    DaysOfOperation = "Weekdays",
                    FirstDepartureTime = new TimeSpan(6, 15, 0),
                    LastDepartureTime = new TimeSpan(21, 15, 0),
                    FrequencyMinutes = 60
                },
                new RouteSchedule
                {
                    DaysOfOperation = "Weekends",
                    FirstDepartureTime = new TimeSpan(9, 15, 0),
                    LastDepartureTime = new TimeSpan(17, 15, 0),
                    FrequencyMinutes = 120
                }
            }
        }
            };

            context.TransitRoutes.AddRange(routes);
            context.SaveChanges();

            var segments = new RouteSegment[]
            {
                new RouteSegment { OriginStopId = stops[0].Id, DestinationStopId = stops[1].Id, RouteId = routes[0].Id, TravelTimeMinutes = 5 },
                new RouteSegment { OriginStopId = stops[1].Id, DestinationStopId = stops[2].Id, RouteId = routes[0].Id, TravelTimeMinutes = 5 },
                new RouteSegment { OriginStopId = stops[2].Id, DestinationStopId = stops[3].Id, RouteId = routes[0].Id, TravelTimeMinutes = 8 },
                new RouteSegment { OriginStopId = stops[3].Id, DestinationStopId = stops[4].Id, RouteId = routes[1].Id, TravelTimeMinutes = 7 },
                new RouteSegment { OriginStopId = stops[4].Id, DestinationStopId = stops[5].Id, RouteId = routes[1].Id, TravelTimeMinutes = 5 },
                new RouteSegment { OriginStopId = stops[5].Id, DestinationStopId = stops[6].Id, RouteId = routes[1].Id, TravelTimeMinutes = 10 }
            };
            
            context.RouteSegments.AddRange(segments);
            context.SaveChanges();

            var routeStops = new RouteStop[]
            {
                new RouteStop { SegmentId = segments[0].Id, StopId = stops[0].Id, StopSequence = 1 },
                new RouteStop { SegmentId = segments[0].Id, StopId = stops[1].Id, StopSequence = 2 },
                new RouteStop { SegmentId = segments[1].Id, StopId = stops[2].Id, StopSequence = 3 },
                new RouteStop { SegmentId = segments[2].Id, StopId = stops[3].Id, StopSequence = 4 },
                new RouteStop { SegmentId = segments[3].Id, StopId = stops[3].Id, StopSequence = 1 },
                new RouteStop { SegmentId = segments[3].Id, StopId = stops[4].Id, StopSequence = 2 },
                new RouteStop { SegmentId = segments[4].Id, StopId = stops[5].Id, StopSequence = 3 },
                new RouteStop { SegmentId = segments[5].Id, StopId = stops[6].Id, StopSequence = 4 }
            };

            context.RouteStops.AddRange(routeStops);
            context.SaveChanges();
        }
    }
}
