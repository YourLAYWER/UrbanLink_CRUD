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

            if (!context.Users.Any(u => u.Email == "conductor@urbanlink.co.za"))
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
                string driverPasswordHash = BCrypt.Net.BCrypt.HashPassword("DriverSecure@2026!", 11);

                var driverUser = new User
                {
                    Name = "Gcina Makhala",
                    Email = "driver@urbanlink.co.za",
                    Password = driverPasswordHash,
                    Role = Models.Enums.Role.Driver,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(driverUser);
                context.SaveChanges();
            }

            if (!context.Users.Any(u => u.Email == "john@gmail.com"))
            {
                string normalUserPasswordHash = BCrypt.Net.BCrypt.HashPassword("JohnSecure@2026!", 11);

                var NormalUser = new User
                {
                    Name = "John Doe",
                    Email = "john@gmail.com",
                    Password = normalUserPasswordHash,
                    Role = Models.Enums.Role.StandardUser,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(NormalUser);
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
                            Status = BusStatus.InMaintenance,
                            ProviderId = provider.Id
                        }
                    );
                    context.SaveChanges();
                }
            }
        }

        public static void SeedTransitSystem(ULDBContext context)
        {
            if (context.Stops.Any()) return;

            var stops = new Stop[]
            {
                new Stop { Name = "Greenacres Ring Rd 3", Latitude = -33.949587, Longitude = 25.581140, IsTransferHub = true, IsActive = true }, // Route 1: Greenacres to Central
                new Stop { Name = "SANBS", Latitude = -33.958742, Longitude = 25.598896, IsTransferHub = false, IsActive = true },
                new Stop { Name = "BP", Latitude = -33.960893, Longitude = 25.602940, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Gqeberha Main St", Latitude = -33.961569, Longitude = 25.624284, IsTransferHub = true, IsActive = true },// Route 2: Central to NMU South
                new Stop { Name = "Kings Beach", Latitude = -33.974092, Longitude = 25.643391, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Shark Rock", Latitude = -33.981637, Longitude = 25.659304, IsTransferHub = false, IsActive = true },
                new Stop { Name = "NMU South Campus", Latitude = -34.009847, Longitude = 25.669267, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Sydenham Stop", Latitude = -33.927480, Longitude = 25.599399, IsTransferHub = false, IsActive = true }, // Route 3: Sydenhem to Gqeberha CBD
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
                new Stop { Name = "Uitenhage Train Station", Latitude = -33.770963, Longitude = 25.409130, IsTransferHub = false, IsActive = true },  // Route 4: Uitenhage station to Gqeberha Train Station
                new Stop { Name = "Alexander Park Train Station", Latitude = -33.781057, Longitude = 25.418472, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Despatch Train Station", Latitude = -33.797757, Longitude = 25.469287, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Azalea Park Train Station", Latitude = -33.815527, Longitude = 25.521027, IsTransferHub = false, IsActive = true },
                new Stop { Name = "RedHouse Train Station", Latitude = -33.837973, Longitude = 25.568188, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Swartkops Train Station", Latitude = -33.867260, Longitude = 25.600640, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Deal Train Station", Latitude = -33.899102, Longitude = 25.607817, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Sydenham train Station", Latitude = -33.923639, Longitude = 25.604296, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Harbour train Station", Latitude = -33.946922, Longitude = 25.610239, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Gqeberha Train Station", Latitude = -33.960411, Longitude = 25.624245, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Sidwell Stop", Latitude = -33.918509, Longitude = 25.599481, IsTransferHub = false, IsActive = true },  // Route 5: Kempston Rd to Greenacres
                new Stop { Name = "Sutton stop", Latitude = -33.923082, Longitude = 25.595497, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Hoch stop", Latitude = -33.926367, Longitude = 25.592519, IsTransferHub = false, IsActive = true },
                new Stop { Name = "Nielson ext stop", Latitude = -33.934139, Longitude = 25.585710, IsTransferHub = false, IsActive = true }
            };

            context.Stops.AddRange(stops);
            context.SaveChanges();

            var provider = context.TransitProviders.FirstOrDefault(p => p.Name == "Algoa Bus");
            var provider2 = context.TransitProviders.FirstOrDefault(p => p.Name == "Libhongolethu");
            var provider3 = context.TransitProviders.FirstOrDefault(p => p.Name == "PRASA");

            if (provider == null)
            {
                provider = new TransitProvider { Name = "Algoa Bus", Type = TransitType.Bus, APIEndpoint = "" };
                provider2 = new TransitProvider { Name = "Libhongolethu", Type = TransitType.Bus, APIEndpoint = "" };
                provider3 = new TransitProvider { Name = "PRASA", Type = TransitType.Train, APIEndpoint = "" };
                context.TransitProviders.Add(provider);
                context.TransitProviders.Add(provider2);
                context.TransitProviders.Add(provider3);
                context.SaveChanges();
            }

            var routes = new TransitRoute[]
            {
                new TransitRoute
                {
                    RouteName = "Route 1: Greenacres to Central",
                    TransportType = TransitType.Bus,
                    BaseFare = 16.00m,
                    ProviderId = provider.Id,
                    RouteMapPath = "xwenEyhc{C\\LVLl@Td@PHBt@Z~@^x@\\@A@??A@?@??A@?@?@??@@?@??@@??@@??@@??@?@@@?@?@?" +
                    "@?@?@A??@TTNVLXJ^@JBH?ZAF?PCP?HEVSn@Kd@K\\Mj@JPHJFB@@|Bz@l@RHa@zCeNDULg@lAqFjAiFNs@nAeGJg@~@kEL" +
                    "s@~CkNF[Ni@vCcKh@}Br@gCV{@h@wBU{@Ic@ACIi@Ge@Ic@Kc@GQIOAAGKGIKIkCaCqCcCLQdA~@J@N@P?PAJCHCHEJIJIN" +
                    "OnCaGVc@Xa@rC}An@i@jAqCbAgBNYDGf@aBDWL{@@KFo@h@t@^f@h@j@J_@XeAHY@IDODSDQDSFa@b@aD@a@h@qDRuA|@{F" +
                    "vA_L^wCBi@?i@e@_GCURQnAs@fB_AfAq@tBsA~@m@v@c@jCwA?C@C@C?CKk@WeADUSwAo@{DwA{IaAyFQkAO{@_@_C_A_GI" +
                    "_@k@gDKi@Ii@a@aCSkAEYAOCWAWA[?OIQEUEMIUCKCE?E?E@G@A@CDEHERKZM\\GHEtBqAHSF]?Cj@uC`@sBFSJ[AA?AAA?" +
                    "AAA?AAA?C?A?A?Au@Ga@?]?a@BG@cAVi@Pk@Ti@Xi@Ze@b@e@b@KJUX",
                    Schedules = new List<RouteSchedule>
                    {
                        new RouteSchedule { DaysOfOperation = "Weekdays", FirstDepartureTime = new TimeSpan(6, 0, 0), LastDepartureTime = new TimeSpan(20, 0, 0), FrequencyMinutes = 60 },
                        new RouteSchedule { DaysOfOperation = "Weekends", FirstDepartureTime = new TimeSpan(8, 0, 0), LastDepartureTime = new TimeSpan(18, 0, 0), FrequencyMinutes = 120 }
                    }
                },
                new TransitRoute
                {
                    RouteName = "Route 2: Central to NMU South",
                    TransportType = TransitType.Bus,
                    BaseFare = 16.00m,
                    ProviderId = provider.Id,
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
                        new RouteSchedule { DaysOfOperation = "Weekdays", FirstDepartureTime = new TimeSpan(6, 15, 0), LastDepartureTime = new TimeSpan(21, 15, 0), FrequencyMinutes = 60 },
                        new RouteSchedule { DaysOfOperation = "Weekends", FirstDepartureTime = new TimeSpan(9, 15, 0), LastDepartureTime = new TimeSpan(17, 15, 0), FrequencyMinutes = 120 }
                    }
                },
                new TransitRoute
                {
                    RouteName = "Route 3: Sydenhem to Gqeberha CBD",
                    IsBidirectional = true,
                    TransportType = TransitType.Bus,
                    BaseFare = 8.00m,
                    ProviderId = provider2.Id,
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
                        new RouteSchedule { Direction = RouteDirections.Inbound, DaysOfOperation = "Weekdays", FirstDepartureTime = new TimeSpan(5, 30, 0), LastDepartureTime = new TimeSpan(19, 0, 0), FrequencyMinutes = 30 },
                        new RouteSchedule { Direction = RouteDirections.Inbound, DaysOfOperation = "Weekends", FirstDepartureTime = new TimeSpan(6, 0, 0), LastDepartureTime = new TimeSpan(17, 0, 0), FrequencyMinutes = 30 },
                        new RouteSchedule { Direction = RouteDirections.Outbound, DaysOfOperation = "Weekdays", FirstDepartureTime = new TimeSpan(5, 30, 0), LastDepartureTime = new TimeSpan(19, 0, 0), FrequencyMinutes = 30 },
                        new RouteSchedule { Direction = RouteDirections.Outbound, DaysOfOperation = "Weekends", FirstDepartureTime = new TimeSpan(6, 0, 0), LastDepartureTime = new TimeSpan(17, 0, 0), FrequencyMinutes = 30 }
                    }
                },
                new TransitRoute
                {
                    RouteName = "Route 4: Uitenhage station to Gqeberha Train Station",
                    TransportType = TransitType.Train,
                    IsBidirectional = true,
                    BaseFare = 8.00m,
                    ProviderId = provider3.Id,
                    RouteMapPath = "v|gnEuyk{Cc@f@eAnA{HdJcAjA]`@W^Y`@QVYb@W`@Wb@[l@[h@_AbByB~Dq@jAsAbCqA~BgB`Dm@fAgAnB[f@Wb@_@f@" +
                    "/[`@]`@c@d@e@b@a@^_@ZYTc@Zo@^mBjAyFhDiDnBuBnAaAl@u@b@u@`@}@`@iBt@w@\\q@Xs@X_A^wB|@{@\\_@L_@Nk@Ne@Lw@P}@Ns@Hs@" +
                    "Ds@FoBLoBN}E\\yE\\mEX{DXgEXqJp@qDVuHh@mBLkALy@Hu@JeBV{AX}A`@eAVcFzAaFxAmA^]J}Bp@w@TmBj@c@LuDfAeEnA_@JiAZe@FW" +
                    "Fk@HgAJm@Di@@]@W@e@?g@Ak@Co@E{@Go@Ks@MmB_@}A[cDo@{Dw@uDu@sBa@eAUmB_@qBa@oAWc@IsBa@w@OwA[yAYuCk@iCi@{@Os@Oc@G" +
                    "g@I}B_@cAO}@Im@Gk@Cm@Em@AuACcDB[?aDFkA@o@Au@Cc@Ea@CgAO{Eo@YCe@EmAIMAcAG_RaCkAOyAWaASy@Oc@Ie@GoAO}G}@yBY{MeB{" +
                    "AQwASmAIeAES?s@Au@?w@Bk@Ba@Bm@Fu@H[Fc@Fg@HkAXeAX[J[Ja@NaA^eBl@{Aj@yAh@}Aj@eDlAkBp@wBv@kC`AyBv@}Aj@sAf@wAh@yB" +
                    "v@s@Ve@PiA`@YLk@Rm@RwAh@WJaA\\}Bx@{Ah@}Al@eBn@cBn@}Aj@]L{@ZUHUJ_@Lg@Rk@Ve@Ts@^_@Pm@VYTWLOHyAp@sAj@sF`CsD~AwA" +
                    "l@aCdAwCnAcBt@o@TyAZk@RgAd@CBWLIDMF]VSNgBjAw@j@e@`@[X_@\\g@h@s@t@kApAoArA{BbCyB~B]^qAtAoArAgBnBkCxCsK`OgLtOo" +
                    "@z@}Vd]iVl\\_AnAaApAoVr\\iCnDU\\GHMVe@p@e@p@_AnAeAvAkA~AaArAmA`B[b@}AvBW^OTSZ]h@OR]h@Wb@a@v@a@v@]t@[x@}@zBgI" +
                    "|SuVdo@uBhFcHtQsJvViIzSqDjJyClHkDfJ}A~DmAzCmC~GmB~EsAhDk@zAWn@Qf@Od@Qj@Sp@Sp@Qn@Ol@wAzFc@fBwA|FkEdQK\\}@pD_B" +
                    "nGa@`BoAbFgCdK_@xAuJh`@w@vCqExQwAxFmDrN{EnR}FrUoDrNoB~HcIr[qLve@{GhXwBtI_ClJqAdFs@zCS|@Ot@Y|AKz@k@dEuAtJuAzJ" +
                    "w@fFi@xDKr@KrAGp@I~AE`B?xA@vAF~BH~A`@rFp@nI^|EFlACfAAj@E|@Kr@Ov@On@Sn@Yr@q@hAyEdH{@nAuGxJ}A|Be@r@MTOVQ\\O^O`" +
                    "@KZGXGZIb@Kp@SvAKl@G^GZIZGRGRGVQb@ITc@~@{AfDiAdCkAlCUf@o@vAcCrFoBlEgBzD}@pBg@jASd@Sj@O`@M`@Ql@]nAOj@YdAU~@g@" +
                    "zB}A`HeArE]hB}@lDwArGoBrImBpIm@hCk@hC]zAi@~BaAdEuBnJiEdRYrAiB`IuAjGwAhGiBbIsAbGg@vBW~@Qp@KZO^KXMZO\\S`@Q^c@v" +
                    "@s@pAaAbB]n@q@jASXUZOPKJKHMJKFMHKFKDIBMDE@MDMBSBM@O@S@Q?QAWCQCOCQGKCKEMGKGKGMIKIKGMMw@o@a@]aDoCyEaE",
                    Schedules = new List<RouteSchedule>
                    {
                        new RouteSchedule { Direction = RouteDirections.Inbound, DaysOfOperation = "Weekdays", FirstDepartureTime = new TimeSpan(5, 30, 0), LastDepartureTime = new TimeSpan(19, 0, 0), FrequencyMinutes = 30 },
                        new RouteSchedule { Direction = RouteDirections.Inbound, DaysOfOperation = "Weekends", FirstDepartureTime = new TimeSpan(6, 0, 0), LastDepartureTime = new TimeSpan(17, 0, 0), FrequencyMinutes = 30 },
                        new RouteSchedule { Direction = RouteDirections.Outbound, DaysOfOperation = "Weekdays", FirstDepartureTime = new TimeSpan(5, 30, 0), LastDepartureTime = new TimeSpan(19, 0, 0), FrequencyMinutes = 30 },
                        new RouteSchedule { Direction = RouteDirections.Outbound, DaysOfOperation = "Weekends", FirstDepartureTime = new TimeSpan(6, 0, 0), LastDepartureTime = new TimeSpan(17, 0, 0), FrequencyMinutes = 30 }
                    }
                },
                new TransitRoute
                {
                    RouteName = "Route 5: Kempston Rd to Greenacres",
                    IsBidirectional = true,
                    TransportType = TransitType.Bus,
                    BaseFare = 13.00m,
                    ProviderId = provider2.Id,
                    RouteMapPath = "fc_nEqlg{C^\\rDtCzBnB~CnCZXpC`C|@v@`Av@\\XPNfClBLJx@r@f@`@NNtF`FnBtATNJFLHJHf@^d@^b@b@PLRPNJX" +
                    "TJJBDNNb@f@t@x@JHb@`@BB\\XZXvCfCTPtBbBhA`A`@\\zAhAXTlB`Br@n@`BvAbAz@VRxApApAdARR~@x@d@^\\Xp@j@|@t@vBhBjA`A|@" +
                    "r@jB`BZRl@h@TPrAnARPp@h@TNnB`BLJPPtA~Ad@x@d@|@N`@@BDJT~@TnAPt@BDVp@NVv@bA`Av@lAn@pDtBHF~EtCb@VdAbAzAxB`AdAf@" +
                    "b@n@`@r@XxLvCJBjCl@pAXPF|Bh@TDTFHa@^LFc@d@_DZF?F?N?HBd@H`@P`@XXLBjCd@",
                    Schedules = new List<RouteSchedule>
                    {
                        new RouteSchedule { Direction = RouteDirections.Inbound, DaysOfOperation = "Weekdays", FirstDepartureTime = new TimeSpan(5, 30, 0), LastDepartureTime = new TimeSpan(19, 0, 0), FrequencyMinutes = 30 },
                        new RouteSchedule { Direction = RouteDirections.Inbound, DaysOfOperation = "Weekends", FirstDepartureTime = new TimeSpan(6, 0, 0), LastDepartureTime = new TimeSpan(17, 0, 0), FrequencyMinutes = 30 },
                        new RouteSchedule { Direction = RouteDirections.Outbound, DaysOfOperation = "Weekdays", FirstDepartureTime = new TimeSpan(5, 30, 0), LastDepartureTime = new TimeSpan(19, 0, 0), FrequencyMinutes = 30 },
                        new RouteSchedule { Direction = RouteDirections.Outbound, DaysOfOperation = "Weekends", FirstDepartureTime = new TimeSpan(6, 0, 0), LastDepartureTime = new TimeSpan(17, 0, 0), FrequencyMinutes = 30 }
                    }
                }
            };

            context.TransitRoutes.AddRange(routes);
            context.SaveChanges();

            var segments = new List<RouteSegment>();
            var routeStops = new List<RouteStop>();

            LinkRouteStops(context, routes[0].Id, new[] { stops[0], stops[1], stops[2], stops[3] }, segments, routeStops);
            LinkRouteStops(context, routes[1].Id, new[] { stops[3], stops[4], stops[5], stops[6] }, segments, routeStops);
            LinkRouteStops(context, routes[2].Id, new[] { stops[7], stops[8], stops[9], stops[10], stops[11], stops[12], stops[13], stops[14], stops[15], stops[16], stops[17], stops[18] }, segments, routeStops);
            LinkRouteStops(context, routes[3].Id, new[] { stops[19], stops[20], stops[21], stops[22], stops[23], stops[24], stops[25], stops[26], stops[27], stops[28] }, segments, routeStops);
            LinkRouteStops(context, routes[4].Id, new[] { stops[29], stops[30], stops[31], stops[32] }, segments, routeStops);

           // context.RouteSegments.AddRange(segments);
            context.RouteStops.AddRange(routeStops);
            context.SaveChanges();
        }

        private static void LinkRouteStops(ULDBContext context, int routeId, Stop[] routeStopSequence, List<RouteSegment> segmentList, List<RouteStop> routeStopList)
        {
            for (int i = 0; i < routeStopSequence.Length; i++)
            {
                var currentStop = routeStopSequence[i];
                int currentSegmentId = 0;

                if (i < routeStopSequence.Length - 1)
                {
                    var nextStop = routeStopSequence[i + 1];
                    var newSegment = new RouteSegment
                    {
                        OriginStopId = currentStop.Id,
                        DestinationStopId = nextStop.Id,
                        RouteId = routeId,
                        TravelTimeMinutes = 5
                    };
                    context.RouteSegments.Add(newSegment);
                    context.SaveChanges();

                    currentSegmentId = newSegment.Id;

                    // ADD THIS LINE: Track the segment so the final stop can find its ID
                    segmentList.Add(newSegment);
                }
                else
                {
                    if (segmentList.Count > 0)
                    {
                        currentSegmentId = segmentList.Last(s => s.RouteId == routeId).Id;
                    }
                }

                routeStopList.Add(new RouteStop
                {
                    SegmentId = currentSegmentId,
                    StopId = currentStop.Id,
                    StopSequence = i + 1
                });
            }
        }
    }
}