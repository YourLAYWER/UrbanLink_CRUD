using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanLink_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "ConductorShifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConductorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BusId = table.Column<int>(type: "int", nullable: false),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    ShiftStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShiftEndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalCashCollected = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
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
                        name: "FK_ConductorShifts_TransitRoute_RouteId",
                        column: x => x.RouteId,
                        principalTable: "TransitRoute",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IncidentReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConductorShiftId = table.Column<int>(type: "int", nullable: false),
                    ReportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsEmergency = table.Column<bool>(type: "bit", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentReport_ConductorShifts_ConductorShiftId",
                        column: x => x.ConductorShiftId,
                        principalTable: "ConductorShifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buses_ProviderId",
                table: "Buses",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ConductorShifts_BusId",
                table: "ConductorShifts",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_ConductorShifts_RouteId",
                table: "ConductorShifts",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReport_ConductorShiftId",
                table: "IncidentReport",
                column: "ConductorShiftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncidentReport");

            migrationBuilder.DropTable(
                name: "ConductorShifts");

            migrationBuilder.DropTable(
                name: "Buses");
        }
    }
}
