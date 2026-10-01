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
                    new TransitProvider { Name = "Libhongolethu", Type = TransitType.Bus, APIEndpoint = "" }
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
                new Stop { Name = "Greenacres Ring Rd 3", Latitude = -33.949587, Longitude = 25.581140, IsTransferHub = true, IsActive = true },
                new Stop { Name = "SANBS", Latitude = -33.958742, Longitude = 25.598896, IsTransferHub = false, IsActive = true },
                new Stop { Name = "BP", Latitude = -33.960893, Longitude = 25.602940, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Gqeberha Main St", Latitude = -33.961569, Longitude = 25.624284, IsTransferHub = true, IsActive = true },
                new Stop { Name = "Kings Beach", Latitude = -33.974092, Longitude = 25.643391, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Shark Rock", Latitude = -33.981637, Longitude = 25.659304, IsTransferHub = false, IsActive = true },
                new Stop { Name = "NMU South Campus", Latitude = -34.009847, Longitude = 25.669267, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Sydenham Stop", Latitude = -33.927480, Longitude = 25.599399, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Johcla Stop", Latitude = -33.932351, Longitude = 25.602753, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Milner Stop", Latitude = -33.935637, Longitude = 25.604378, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Quick Stop", Latitude = -33.940590, Longitude = 25.605469, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Magistrates Stop", Latitude = -33.944380, Longitude = 25.605983, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Pier 14 Stop", Latitude = -33.949903, Longitude = 25.607871, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Post Office Stop", Latitude = -33.954781, Longitude = 25.613274, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Hartman Stop", Latitude = -33.956828, Longitude = 25.617586, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Bongo Stop", Latitude = -33.957280, Longitude = 25.618224, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Goven Mbeki Stop", Latitude = -33.957693, Longitude = 25.618814, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Harbour Stop", Latitude = -33.963109, Longitude = 25.626052, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Tramway Stop", Latitude = -33.965053, Longitude = 25.626133, IsTransferHub = false, IsActive = true },

            };

            context.Stops.AddRange(stops);
            context.SaveChanges();

            // ==========================================
            // Fetch Algoa Bus instead of creating a new provider
            // ==========================================
            var provider = context.TransitProviders.FirstOrDefault(p => p.Name == "Algoa Bus");

            // Fetch Libhongolethu instead of creating a new provider
            var provider2 = context.TransitProviders.FirstOrDefault(p => p.Name == "Libhongolethu");

            // Safety check just in case the providers haven't seeded yet
            if (provider == null)
            {
                provider = new TransitProvider { Name = "Algoa Bus", Type = TransitType.Bus, APIEndpoint = "" };
                provider2 = new TransitProvider { Name = "Libhongolethu", Type = TransitType.Bus, APIEndpoint = "" };
                context.TransitProviders.Add(provider);
                context.TransitProviders.Add(provider2);
                context.SaveChanges();
            }

            // ==========================================
            // 4. Transit Routes with RouteSchedules
            // ==========================================
            var routes = new TransitRoute[]
            {
            new TransitRoute
            {
                RouteName = "Route 1: Greenacres to Central",
                TransportType = TransitType.Bus,
                BaseFare = 16.00m,
                ProviderId = provider.Id, // Links directly to Algoa Bus!
                RouteMapPath = "xwenEyhc{C\\LVLl@Td@PHBt@Z~@^x@\\@A@??A@?@??A@?@?@??@@?@??@@??@@??@@??@?@@@?@?@?" +
                "@?@?@A??@TTNVLXJ^@JBH?ZAF?PCP?HEVSn@Kd@K\\Mj@JPHJFB@@|Bz@l@RHa@zCeNDULg@lAqFjAiFNs@nAeGJg@~@kEL" +
                "s@~CkNF[Ni@vCcKh@}Br@gCV{@h@wBU{@Ic@ACIi@Ge@Ic@Kc@GQIOAAGKGIKIkCaCqCcCLQdA~@J@N@P?PAJCHCHEJIJIN" +
                "OnCaGVc@Xa@rC}An@i@jAqCbAgBNYDGf@aBDWL{@@KFo@h@t@^f@h@j@J_@XeAHY@IDODSDQDSFa@b@aD@a@h@qDRuA|@{F" +
                "vA_L^wCBi@?i@e@_GCURQnAs@fB_AfAq@tBsA~@m@v@c@jCwA?C@C@C?CKk@WeADUSwAo@{DwA{IaAyFQkAO{@_@_C_A_GI" +
                "_@k@gDKi@Ii@a@aCSkAEYAOCWAWA[?OIQEUEMIUCKCE?E?E@G@A@CDEHERKZM\\GHEtBqAHSF]?Cj@uC`@sBFSJ[AA?AAA?" +
                "AAA?AAA?C?A?A?Au@Ga@?]?a@BG@cAVi@Pk@Ti@Xi@Ze@b@e@b@KJUX",
                Schedules = new List<RouteSchedule>
                {
                   new RouteSchedule
                   {
                        DaysOfOperation = "Weekdays",
                        FirstDepartureTime = new TimeSpan(6, 0, 0),
                        LastDepartureTime = new TimeSpan(20, 0, 0),
                        FrequencyMinutes = 60
                   },
                    new RouteSchedule
                    {
                        DaysOfOperation = "Weekends",
                        FirstDepartureTime = new TimeSpan(8, 0, 0),
                        LastDepartureTime = new TimeSpan(18, 0, 0),
                        FrequencyMinutes = 120
                    }
                }
            },
            new TransitRoute
            {
                RouteName = "Route 2: Central to NMU South",
                TransportType = TransitType.Bus,
                BaseFare = 16.00m,
                ProviderId = provider.Id, // Links directly to Algoa Bus!
                RouteMapPath = "`bhnEqyk{CKLA?WZKLKNq@|@ABEDeAnAWTCBgAjAq@n@MLy@x@c@`@o@l@WVIJo@t@W^eAtACDa@l@SXQ`@A@O^" +
                "KXKZLLFDd@b@JH\\\\XZJJMDGBKJ?J]Ge@IOAWGMEIE[MGCAC?CAA?C?IDWBI?AH[BEBMHSNg@Rk@HWNc@Rg@R]DK~AwBTWLKh@g@BO@A@A?AX" +
                "[t@y@hG_H@AhA{AP_@@CLi@FDJDVJB?BB\\]d@c@f@a@h@[@?f@Yj@Uh@QbAW~@SjBUx@[@Ap@c@P[HOHMNOPORMb@YLGfAw@PMjA_AZWZYPOF" +
                "GNMNODERSX]RU^_@`@g@\\a@RQHKLIJGJCJEHAF?FAH?J?H@XFNFHDJFDFB@DDFFFJL\\t@k@l@g@h@i@l@aAXk@v@yBNe@HSNg@z@uC`AuDTg" +
                "AFUpAoFLg@ZuAr@{Cf@sBTcAFOJw@^{A\\uATu@NWhC_IHYDO@O?WBu@EgA?EAa@?k@?E?U@QFYFOl@gBNc@fAiCd@mApBsEfBkE~BeGFSXiA@" +
                "I\\sBj@}Df@kBDQd@uAt@{B?Al@_BXi@LSNS`@g@f@c@rBsBZa@NUBCVg@Rm@H[BGH_@BOHq@X}ENkAX}@Xg@JKNOlAsAVc@Xg@b@q@JMb@iAf" +
                "@oB?CXaAd@aB@Gb@kB@EXkBv@iFVsBDQBKDERqAxAgJTmB@Eh@sBNc@^aAzAqEdB_En@sAdAkBjAqAf@k@`CoCHKNMpAiAvAkAhA_An@i@DEPMr" +
                "@k@rC}CbAkAbAkArBwBjCyCHLhFlHf@z@nA`B`IvILLHJ\\Z\\PPHDBNFXLHBLBLBTDJBFBJFnAN|I~@jATvANB?`CT`@BJBLDJFFDFF?A@A?A" +
                "@A@A@A@??A@?@A@?@?@?@?@?@?@??@@?@??@@??@@??@@??@@??@@@BCDCHCJAzL?dF@tEErB?T?rAA`C?\\DPDPHp@d@BBHDl@f@TJVFZ@lFA`@?`DA",
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
            },
            new TransitRoute
            {
                RouteName = "Route 3: Sydenhem to Gqeberha CBD",
                TransportType = TransitType.Bus,
                BaseFare = 8.00m,
                ProviderId = provider2.Id, // Links directly to Libhongolethu
                RouteMapPath = "~kanE}yf{CnH}Ip@q@z@i@TIlDoAnCcAjCaAh@U\\KhBq@h@Sb@O`A_@~CiAn@Ur@YlAc" +
                "@TI@A^OFCHEBAHG@AHEPO\\YXU~@}@XCbACf@C^ENAn@GVC^Cj@El@ELA`@Cp@ENAHALAjAIJAl@EhCQj@Cv" +
                "BMf@EFA\\CbAG|@GbAIVEPCtAMn@GlAIpAKb@EbBK`AIVC^ClAI^CjAG`AI^E`CO`AILAHAD?DCFCFCPKj@]" +
                "p@a@d@Y`Am@FGfAo@`BeAj@]FEx@i@FEjAu@`BuA^[`@]|AkA|B_BTOfAw@h@]NQHMJOZg@@CZi@HOJQJQHO" +
                "HSDIFODMFOFSDMDMDOF[BKDQDUF_@RkAPkAFYHe@He@Po@Lk@Ja@DUJg@DSFSFOHUHSJUHQJQ\\o@bA{Af@w" +
                "@JOZc@hD{F`@m@`@s@FKRa@e@IOAWGMEIE[MGCAC?CAA?C?IDWBI?AH[BEBMHSNg@Rk@HWNc@Rg@R]DK~AwB" +
                "TWLKh@g@BCrAoAhAgA?AbA{@f@m@TW`BeBhAyAHIZa@LO\\]d@c@f@a@h@[@?CGUu@Ea@k@wCIi@Hh@j@vCD" +
                "`@Tt@BFf@Yj@Uh@QbAW~@SjBUx@[@Ap@c@P[HOHMNOPORMb@YLGJz@@@Hl@?@Fz@MFe@RSH",
                Schedules = new List<RouteSchedule>
                {
                    new RouteSchedule
                    {
                        DaysOfOperation = "Weekdays",
                        FirstDepartureTime = new TimeSpan(5, 30, 0),
                        LastDepartureTime = new TimeSpan(19, 0, 0),
                        FrequencyMinutes = 30
                    },
                    new RouteSchedule
                    {
                        DaysOfOperation = "Weekends",
                        FirstDepartureTime = new TimeSpan(6, 0, 0),
                        LastDepartureTime = new TimeSpan(17, 0, 0),
                        FrequencyMinutes = 30
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
                new RouteSegment { OriginStopId = stops[2].Id, DestinationStopId = stops[3].Id, RouteId = routes[0].Id, TravelTimeMinutes = 8 },// Route 1 segments
                new RouteSegment { OriginStopId = stops[3].Id, DestinationStopId = stops[4].Id, RouteId = routes[1].Id, TravelTimeMinutes = 7 },
                new RouteSegment { OriginStopId = stops[4].Id, DestinationStopId = stops[5].Id, RouteId = routes[1].Id, TravelTimeMinutes = 5 },
                new RouteSegment { OriginStopId = stops[5].Id, DestinationStopId = stops[6].Id, RouteId = routes[1].Id, TravelTimeMinutes = 10 }, // Route 2 segments
                new RouteSegment { OriginStopId = stops[7].Id, DestinationStopId = stops[8].Id, RouteId = routes[2].Id, TravelTimeMinutes = 4 },
                new RouteSegment { OriginStopId = stops[8].Id, DestinationStopId = stops[9].Id, RouteId = routes[2].Id, TravelTimeMinutes = 3 },
                new RouteSegment { OriginStopId = stops[9].Id, DestinationStopId = stops[10].Id, RouteId = routes[2].Id, TravelTimeMinutes = 3 },
                new RouteSegment { OriginStopId = stops[10].Id, DestinationStopId = stops[11].Id, RouteId = routes[2].Id, TravelTimeMinutes = 3 },
                new RouteSegment { OriginStopId = stops[11].Id, DestinationStopId = stops[12].Id, RouteId = routes[2].Id, TravelTimeMinutes = 3 },
                new RouteSegment { OriginStopId = stops[12].Id, DestinationStopId = stops[13].Id, RouteId = routes[2].Id, TravelTimeMinutes = 3 },
                new RouteSegment { OriginStopId = stops[13].Id, DestinationStopId = stops[14].Id, RouteId = routes[2].Id, TravelTimeMinutes = 3 },
                new RouteSegment { OriginStopId = stops[14].Id, DestinationStopId = stops[15].Id, RouteId = routes[2].Id, TravelTimeMinutes = 3 },
                new RouteSegment { OriginStopId = stops[15].Id, DestinationStopId = stops[16].Id, RouteId = routes[2].Id, TravelTimeMinutes = 3 },
                new RouteSegment { OriginStopId = stops[16].Id, DestinationStopId = stops[17].Id, RouteId = routes[2].Id, TravelTimeMinutes = 5 },
                new RouteSegment { OriginStopId = stops[17].Id, DestinationStopId = stops[18].Id, RouteId = routes[2].Id, TravelTimeMinutes = 5 }, // Route 3 segments
            };
            
            context.RouteSegments.AddRange(segments);
            context.SaveChanges();

            var routeStops = new RouteStop[]
            {
                new RouteStop { SegmentId = segments[0].Id, StopId = stops[0].Id, StopSequence = 1 },
                new RouteStop { SegmentId = segments[0].Id, StopId = stops[1].Id, StopSequence = 2 },
                new RouteStop { SegmentId = segments[1].Id, StopId = stops[2].Id, StopSequence = 3 },
                new RouteStop { SegmentId = segments[2].Id, StopId = stops[3].Id, StopSequence = 4 }, // Route 1 stops
                new RouteStop { SegmentId = segments[3].Id, StopId = stops[3].Id, StopSequence = 1 },
                new RouteStop { SegmentId = segments[3].Id, StopId = stops[4].Id, StopSequence = 2 },
                new RouteStop { SegmentId = segments[4].Id, StopId = stops[5].Id, StopSequence = 3 },
                new RouteStop { SegmentId = segments[5].Id, StopId = stops[6].Id, StopSequence = 4 }, // Route 2 stops
                new RouteStop { SegmentId = segments[6].Id, StopId = stops[7].Id, StopSequence = 1 },
                new RouteStop { SegmentId = segments[7].Id, StopId = stops[8].Id, StopSequence = 2 },
                new RouteStop { SegmentId = segments[8].Id, StopId = stops[9].Id, StopSequence = 3 },
                new RouteStop { SegmentId = segments[9].Id, StopId = stops[10].Id, StopSequence = 4 },
                new RouteStop { SegmentId = segments[10].Id, StopId = stops[11].Id, StopSequence = 5 },
                new RouteStop { SegmentId = segments[11].Id, StopId = stops[12].Id, StopSequence = 6 },
                new RouteStop { SegmentId = segments[12].Id, StopId = stops[13].Id, StopSequence = 7 },
                new RouteStop { SegmentId = segments[13].Id, StopId = stops[14].Id, StopSequence = 8 },
                new RouteStop { SegmentId = segments[14].Id, StopId = stops[15].Id, StopSequence = 9 },
                new RouteStop { SegmentId = segments[15].Id, StopId = stops[16].Id, StopSequence = 10 },
                new RouteStop { SegmentId = segments[16].Id, StopId = stops[17].Id, StopSequence = 11 },
                new RouteStop { SegmentId = segments[17].Id, StopId = stops[18].Id, StopSequence = 12 }, // Route 3 stops
            };
            context.RouteStops.AddRange(routeStops);
            context.SaveChanges();
        }
    }
}
