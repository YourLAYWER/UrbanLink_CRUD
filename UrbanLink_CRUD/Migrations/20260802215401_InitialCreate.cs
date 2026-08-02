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
                name: "JourneyLegs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SequenceOrder = table.Column<int>(type: "int", nullable: false),
                    JourneyID = table.Column<int>(type: "int", nullable: false),
                    SegmentID = table.Column<int>(type: "int", nullable: false),
                    RouteSegmentID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JourneyLegs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_JourneyLegs_Journeys_JourneyID",
                        column: x => x.JourneyID,
                        principalTable: "Journeys",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JourneyLegs_RouteSegments_RouteSegmentID",
                        column: x => x.RouteSegmentID,
                        principalTable: "RouteSegments",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "RouteStops",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SegmentID = table.Column<int>(type: "int", nullable: false),
                    StopID = table.Column<int>(type: "int", nullable: false),
                    StopSequence = table.Column<int>(type: "int", nullable: false),
                    RouteSegmentID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteStops", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RouteStops_RouteSegments_RouteSegmentID",
                        column: x => x.RouteSegmentID,
                        principalTable: "RouteSegments",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_RouteStops_Stops_StopID",
                        column: x => x.StopID,
                        principalTable: "Stops",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JourneyLegs_JourneyID",
                table: "JourneyLegs",
                column: "JourneyID");

            migrationBuilder.CreateIndex(
                name: "IX_JourneyLegs_RouteSegmentID",
                table: "JourneyLegs",
                column: "RouteSegmentID");

            migrationBuilder.CreateIndex(
                name: "IX_RouteStops_RouteSegmentID",
                table: "RouteStops",
                column: "RouteSegmentID");

            migrationBuilder.CreateIndex(
                name: "IX_RouteStops_StopID",
                table: "RouteStops",
                column: "StopID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JourneyLegs");

            migrationBuilder.DropTable(
                name: "RouteStops");
        }
    }
}
