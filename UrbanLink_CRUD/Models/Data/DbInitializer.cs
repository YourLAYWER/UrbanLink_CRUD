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

            var provider = new TransitProvider
            {
                Name = "Nelson Mandela Bay Municipality Transit",
                Type = "Bus",
                APIEndpoint = "https://api.urbanlink.co.za/v1/municipal"
            };

            context.TransitProviders.Add(provider);
            context.SaveChanges();

            // ==========================================
            // 4. UPDATED: Transit Routes with RouteSchedules
            // ==========================================
            var routes = new TransitRoute[]
            {
                new TransitRoute
                {
                    RouteName = "Route 1: Greenacres to Central",
                    TransportType = "Bus",
                    BaseFare = 15.00m,
                    ProviderId = provider.Id,
                    // Injecting the schedules directly into the route!
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
                            FrequencyMinutes = 60 // Less frequent on weekends
                        }
                    }
                },
                new TransitRoute
                {
                    RouteName = "Route 2: Central to NMU South",
                    TransportType = "Bus",
                    BaseFare = 15.00m,
                    ProviderId = provider.Id,
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
                            FrequencyMinutes = 120 // Runs every 2 hours on weekends
                        }
                    }
                }
            };

            context.TransitRoutes.AddRange(routes);
            context.SaveChanges();

            var segments = new RouteSegment[]
            {
                new RouteSegment {
                    OriginStopId = stops[0].Id, DestinationStopId = stops[1].Id, RouteId = routes[0].Id,
                    TravelTimeMinutes = 5
                },
                new RouteSegment {
                    OriginStopId = stops[1].Id, DestinationStopId = stops[2].Id, RouteId = routes[0].Id,
                    TravelTimeMinutes = 5
                },
                new RouteSegment {
                    OriginStopId = stops[2].Id, DestinationStopId = stops[3].Id, RouteId = routes[0].Id,
                    TravelTimeMinutes = 8
                },

                new RouteSegment {
                    OriginStopId = stops[3].Id, DestinationStopId = stops[4].Id, RouteId = routes[1].Id,
                    TravelTimeMinutes = 7
                },
                new RouteSegment {
                    OriginStopId = stops[4].Id, DestinationStopId = stops[5].Id, RouteId = routes[1].Id,
                    TravelTimeMinutes = 5
                },
                new RouteSegment {
                    OriginStopId = stops[5].Id, DestinationStopId = stops[6].Id, RouteId = routes[1].Id,
                    TravelTimeMinutes = 10
                }
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
