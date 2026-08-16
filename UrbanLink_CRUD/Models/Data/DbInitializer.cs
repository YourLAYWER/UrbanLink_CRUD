using BCrypt.Net;
using UrbanLink_CRUD.Models.Entities;
using UrbanLink_CRUD.Models.Entities.Transit;
using UrbanLink_CRUD.Models.Enums;


namespace UrbanLink_CRUD.Models.Data
{
    public class DbInitializer
    {
        // 1. Check if the admin already exists so we don't duplicate it on every run
        public static void SeedAdminUser(ULDBContext context)
        {
            // 1. Check if the admin already exists so we don't duplicate it on every run
            if (!context.Users.Any(u => u.Email == "admin@urbanlink.co.za"))
            {
                // 2. Hash the default admin password
                string adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("AdminSecure@2026!", 11);

                // 3. Create the admin user
                var adminUser = new User
                {
                    Name = "System Administrator",
                    Email = "admin@urbanlink.co.za",
                    Password = adminPasswordHash,
                    CommuterProfile = "Admin", // This is the crucial role for your [Authorize] tags
                    CreatedAt = DateTime.UtcNow
                };

                // 4. Save to the database
                context.Users.Add(adminUser);
                context.SaveChanges();
            }
        }

         public static void SeedTransitProviders(ULDBContext context)
        {
            if (!context.TransitProviders.Any())
            {
                context.TransitProviders.AddRange(
                    new TransitProvider { Name = "Algoa Bus", Type = "Bus", APIEndpoint = "" },
                    new TransitProvider { Name = "PRASA", Type = "Rail", APIEndpoint = "" },
                    new TransitProvider { Name = "Gqeberha Municipal Bus", Type = "Bus", APIEndpoint = "" }
                );
                context.SaveChanges();
            }
        }
        public static void SeedTransitSystem(ULDBContext context)
        {
            // 1. Exit if the database already has data
            if (context.Stops.Any())
            {
                return;
            }

            // 2. Create the exact Stops requested
            // Note: Gqeberha Central is flagged as a Transfer Hub to allow connecting Route 1 and Route 2
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
            context.SaveChanges(); // Generates Stop IDs

            // 3. Create the Transit Provider
            var provider = new TransitProvider
            {
                Name = "Nelson Mandela Bay Municipality Transit",
                Type = "Bus",
                APIEndpoint = "https://api.urbanlink.co.za/v1/municipal"
            };

            context.TransitProviders.Add(provider);
            context.SaveChanges(); // Generates Provider ID

            // 4. Create the two Transit Routes with the NEW Schedule Intervals
            var routes = new TransitRoute[]
            {
        new TransitRoute
        {
            RouteName = "Route 1: Greenacres to Central",
            TransportType = "Bus",
            BaseFare = 15.00m,
            ProviderId = provider.Id,
            
            // NEW: Runs every 30 mins from 6:00 AM to 8:00 PM
            FirstDepartureTime = new TimeSpan(6, 0, 0),
            LastDepartureTime = new TimeSpan(20, 0, 0),
            FrequencyMinutes = 30
        },
        new TransitRoute
        {
            RouteName = "Route 2: Central to NMU South",
            TransportType = "Bus",
            BaseFare = 15.00m,
            ProviderId = provider.Id,
            
            // NEW: Runs every 60 mins at a quarter past, from 6:15 AM to 9:15 PM
            FirstDepartureTime = new TimeSpan(6, 15, 0),
            LastDepartureTime = new TimeSpan(21, 15, 0),
            FrequencyMinutes = 60
        }
            };

            context.TransitRoutes.AddRange(routes);
            context.SaveChanges(); // Generates Route IDs

            // 5. Connect the Stops with Route Segments to build the directed graph
            var segments = new RouteSegment[]
            {
        // ==========================================
        // ROUTE 1: Greenacres -> SABS -> BP -> Central
        // ==========================================
        new RouteSegment {
            OriginStopId = stops[0].Id, DestinationStopId = stops[1].Id, RouteId = routes[0].Id,
            TravelTimeMinutes = 5
        }, // Greenacres -> SABS
        
        new RouteSegment {
            OriginStopId = stops[1].Id, DestinationStopId = stops[2].Id, RouteId = routes[0].Id,
            TravelTimeMinutes = 5
        }, // SABS -> BP
        
        new RouteSegment {
            OriginStopId = stops[2].Id, DestinationStopId = stops[3].Id, RouteId = routes[0].Id,
            TravelTimeMinutes = 8
        }, // BP -> Central

        // ==========================================
        // ROUTE 2: Central -> KingsBeach -> Boardwalk -> NMU
        // ==========================================
        new RouteSegment {
            OriginStopId = stops[3].Id, DestinationStopId = stops[4].Id, RouteId = routes[1].Id,
            TravelTimeMinutes = 7
        }, // Central -> KingsBeach
        
        new RouteSegment {
            OriginStopId = stops[4].Id, DestinationStopId = stops[5].Id, RouteId = routes[1].Id,
            TravelTimeMinutes = 5
        }, // KingsBeach -> Boardwalk
        
        new RouteSegment {
            OriginStopId = stops[5].Id, DestinationStopId = stops[6].Id, RouteId = routes[1].Id,
            TravelTimeMinutes = 10
        }  // Boardwalk -> NMU
            };

            context.RouteSegments.AddRange(segments);
            context.SaveChanges();

            // 6. Connect the Route Stops
            var routeStops = new RouteStop[]
            {
        // Route 1 sequence
        new RouteStop { SegmentId = segments[0].Id, StopId = stops[0].Id, StopSequence = 1 },
        new RouteStop { SegmentId = segments[0].Id, StopId = stops[1].Id, StopSequence = 2 },
        new RouteStop { SegmentId = segments[1].Id, StopId = stops[2].Id, StopSequence = 3 },
        new RouteStop { SegmentId = segments[2].Id, StopId = stops[3].Id, StopSequence = 4 },

        // Route 2 sequence
        new RouteStop { SegmentId = segments[3].Id, StopId = stops[3].Id, StopSequence = 1 },
        new RouteStop { SegmentId = segments[3].Id, StopId = stops[4].Id, StopSequence = 2 },
        new RouteStop { SegmentId = segments[4].Id, StopId = stops[5].Id, StopSequence = 3 },
        new RouteStop { SegmentId = segments[5].Id, StopId = stops[6].Id, StopSequence = 4 }
            };

            context.RouteStops.AddRange(routeStops);
            context.SaveChanges();
        }



        public static void SeedTickets(ULDBContext context)
        {
            if (!context.Tickets.Any())
            {
                context.Tickets.AddRange(
                    new Ticket
                    {
                        QRCodeData = "QR-1001",
                        Status = TicketStatus.Active
                    },
                    new Ticket
                    {
                        QRCodeData = "QR-1002",
                        Status = TicketStatus.Active
                    }
                );

                context.SaveChanges();
            }
        }
    }
}
