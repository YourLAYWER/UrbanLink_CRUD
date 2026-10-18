using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanLink_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniversalName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageURL = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductID);
                });

            migrationBuilder.CreateTable(
                name: "Retailers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CollectionAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Retailers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    IsTransferHub = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stops", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    TicketId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QRCodeData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.TicketId);
                });

            migrationBuilder.CreateTable(
                name: "TransitProviders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    APIEndpoint = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransitProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RetailerProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RetailerId = table.Column<int>(type: "int", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InStock = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetailerProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RetailerProducts_Retailers_RetailerId",
                        column: x => x.RetailerId,
                        principalTable: "Retailers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Journeys",
                columns: table => new
                {
                    JourneyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TicketReference = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RedeemedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndStopID = table.Column<int>(type: "int", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Journeys", x => x.JourneyID);
                    table.ForeignKey(
                        name: "FK_Journeys_Stops_EndStopID",
                        column: x => x.EndStopID,
                        principalTable: "Stops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TripDestinations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShiftId = table.Column<int>(type: "int", nullable: false),
                    StopId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PassengersDroppingOff = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripDestinations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripDestinations_Stops_StopId",
                        column: x => x.StopId,
                        principalTable: "Stops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Buses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FleetNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LicensePlate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeatingCapacity = table.Column<int>(type: "int", nullable: false),
                    StandingCapacity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Buses_TransitProviders_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "TransitProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransitRoute",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBidirectional = table.Column<bool>(type: "bit", nullable: false),
                    TransportType = table.Column<int>(type: "int", nullable: false),
                    BaseFare = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    RouteMapPath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransitRoute", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransitRoute_TransitProviders_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "TransitProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    CartID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.CartID);
                    table.ForeignKey(
                        name: "FK_Carts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportedByUserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdminNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidents_Users_ReportedById",
                        column: x => x.ReportedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RetailerId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FulfillmentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FulfillmentFeedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Retailers_RetailerId",
                        column: x => x.RetailerId,
                        principalTable: "Retailers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserJourneys",
                columns: table => new
                {
                    JourneyID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    StartStopID = table.Column<int>(type: "int", nullable: false),
                    EstimatedArrivalTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserJourneys", x => x.JourneyID);
                    table.ForeignKey(
                        name: "FK_UserJourneys_Journeys_JourneyID",
                        column: x => x.JourneyID,
                        principalTable: "Journeys",
                        principalColumn: "JourneyID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserJourneys_Stops_StartStopID",
                        column: x => x.StartStopID,
                        principalTable: "Stops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserJourneys_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConductorShifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConductorId = table.Column<int>(type: "int", nullable: false),
                    BusId = table.Column<int>(type: "int", nullable: false),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    ShiftStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShiftEndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalCashCollected = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DigitalTicketScanned = table.Column<int>(type: "int", nullable: false),
                    BusId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConductorShifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConductorShifts_Buses_BusId",
                        column: x => x.BusId,
                        principalTable: "Buses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConductorShifts_Buses_BusId1",
                        column: x => x.BusId1,
                        principalTable: "Buses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConductorShifts_TransitRoute_RouteId",
                        column: x => x.RouteId,
                        principalTable: "TransitRoute",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConductorShifts_Users_ConductorId",
                        column: x => x.ConductorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JourneyLegs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JourneyId = table.Column<int>(type: "int", nullable: false),
                    SequenceOrder = table.Column<int>(type: "int", nullable: false),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    BoardingStopId = table.Column<int>(type: "int", nullable: false),
                    DestinationStopId = table.Column<int>(type: "int", nullable: false),
                    LegFare = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LegDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    TicketReference = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RedeemedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JourneyLegs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JourneyLegs_Journeys_JourneyId",
                        column: x => x.JourneyId,
                        principalTable: "Journeys",
                        principalColumn: "JourneyID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JourneyLegs_Stops_BoardingStopId",
                        column: x => x.BoardingStopId,
                        principalTable: "Stops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JourneyLegs_Stops_DestinationStopId",
                        column: x => x.DestinationStopId,
                        principalTable: "Stops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JourneyLegs_TransitRoute_RouteId",
                        column: x => x.RouteId,
                        principalTable: "TransitRoute",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RouteSchedules",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    DaysOfOperation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstDepartureTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    LastDepartureTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    FrequencyMinutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteSchedules", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK_RouteSchedules_TransitRoute_RouteId",
                        column: x => x.RouteId,
                        principalTable: "TransitRoute",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RouteSegments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginStopId = table.Column<int>(type: "int", nullable: false),
                    DestinationStopId = table.Column<int>(type: "int", nullable: false),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    TravelTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    TransitProviderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteSegments_Stops_DestinationStopId",
                        column: x => x.DestinationStopId,
                        principalTable: "Stops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteSegments_Stops_OriginStopId",
                        column: x => x.OriginStopId,
                        principalTable: "Stops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteSegments_TransitProviders_TransitProviderId",
                        column: x => x.TransitProviderId,
                        principalTable: "TransitProviders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RouteSegments_TransitRoute_RouteId",
                        column: x => x.RouteId,
                        principalTable: "TransitRoute",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    CartItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CartID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.CartItemID);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_CartID",
                        column: x => x.CartID,
                        principalTable: "Carts",
                        principalColumn: "CartID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    OrderItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.OrderItemID);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CashJourneys",
                columns: table => new
                {
                    JourneyID = table.Column<int>(type: "int", nullable: false),
                    ShiftID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashJourneys", x => x.JourneyID);
                    table.ForeignKey(
                        name: "FK_CashJourneys_ConductorShifts_ShiftID",
                        column: x => x.ShiftID,
                        principalTable: "ConductorShifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashJourneys_Journeys_JourneyID",
                        column: x => x.JourneyID,
                        principalTable: "Journeys",
                        principalColumn: "JourneyID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncidentReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConductorShiftId = table.Column<int>(type: "int", nullable: false),
                    ReportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsEmergency = table.Column<bool>(type: "bit", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentReports_ConductorShifts_ConductorShiftId",
                        column: x => x.ConductorShiftId,
                        principalTable: "ConductorShifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RouteStops",
                columns: table => new
                {
                    SegmentId = table.Column<int>(type: "int", nullable: false),
                    StopId = table.Column<int>(type: "int", nullable: false),
                    StopSequence = table.Column<int>(type: "int", nullable: false),
                    RouteSegmentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteStops", x => new { x.SegmentId, x.StopId });
                    table.ForeignKey(
                        name: "FK_RouteStops_RouteSegments_RouteSegmentId",
                        column: x => x.RouteSegmentId,
                        principalTable: "RouteSegments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RouteStops_RouteSegments_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "RouteSegments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteStops_Stops_StopId",
                        column: x => x.StopId,
                        principalTable: "Stops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buses_ProviderId",
                table: "Buses",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartID",
                table: "CartItems",
                column: "CartID");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductID",
                table: "CartItems",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                table: "Carts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CashJourneys_ShiftID",
                table: "CashJourneys",
                column: "ShiftID");

            migrationBuilder.CreateIndex(
                name: "IX_ConductorShifts_BusId",
                table: "ConductorShifts",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_ConductorShifts_BusId1",
                table: "ConductorShifts",
                column: "BusId1");

            migrationBuilder.CreateIndex(
                name: "IX_ConductorShifts_ConductorId",
                table: "ConductorShifts",
                column: "ConductorId");

            migrationBuilder.CreateIndex(
                name: "IX_ConductorShifts_RouteId",
                table: "ConductorShifts",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReports_ConductorShiftId",
                table: "IncidentReports",
                column: "ConductorShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_ReportedById",
                table: "Incidents",
                column: "ReportedById");

            migrationBuilder.CreateIndex(
                name: "IX_JourneyLegs_BoardingStopId",
                table: "JourneyLegs",
                column: "BoardingStopId");

            migrationBuilder.CreateIndex(
                name: "IX_JourneyLegs_DestinationStopId",
                table: "JourneyLegs",
                column: "DestinationStopId");

            migrationBuilder.CreateIndex(
                name: "IX_JourneyLegs_JourneyId",
                table: "JourneyLegs",
                column: "JourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_JourneyLegs_RouteId",
                table: "JourneyLegs",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Journeys_EndStopID",
                table: "Journeys",
                column: "EndStopID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderID",
                table: "OrderItems",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductID",
                table: "OrderItems",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_RetailerId",
                table: "Orders",
                column: "RetailerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RetailerProducts_RetailerId",
                table: "RetailerProducts",
                column: "RetailerId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteSchedules_RouteId",
                table: "RouteSchedules",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteSegments_DestinationStopId",
                table: "RouteSegments",
                column: "DestinationStopId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteSegments_OriginStopId",
                table: "RouteSegments",
                column: "OriginStopId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteSegments_RouteId",
                table: "RouteSegments",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteSegments_TransitProviderId",
                table: "RouteSegments",
                column: "TransitProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteStops_RouteSegmentId",
                table: "RouteStops",
                column: "RouteSegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteStops_StopId",
                table: "RouteStops",
                column: "StopId");

            migrationBuilder.CreateIndex(
                name: "IX_TransitRoute_ProviderId",
                table: "TransitRoute",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_TripDestinations_StopId",
                table: "TripDestinations",
                column: "StopId");

            migrationBuilder.CreateIndex(
                name: "IX_UserJourneys_StartStopID",
                table: "UserJourneys",
                column: "StartStopID");

            migrationBuilder.CreateIndex(
                name: "IX_UserJourneys_UserID",
                table: "UserJourneys",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "CashJourneys");

            migrationBuilder.DropTable(
                name: "IncidentReports");

            migrationBuilder.DropTable(
                name: "Incidents");

            migrationBuilder.DropTable(
                name: "JourneyLegs");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "RetailerProducts");

            migrationBuilder.DropTable(
                name: "RouteSchedules");

            migrationBuilder.DropTable(
                name: "RouteStops");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "TripDestinations");

            migrationBuilder.DropTable(
                name: "UserJourneys");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "ConductorShifts");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "RouteSegments");

            migrationBuilder.DropTable(
                name: "Journeys");

            migrationBuilder.DropTable(
                name: "Buses");

            migrationBuilder.DropTable(
                name: "Retailers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "TransitRoute");

            migrationBuilder.DropTable(
                name: "Stops");

            migrationBuilder.DropTable(
                name: "TransitProviders");
        }
    }
}
