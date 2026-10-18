using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanLink_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class initialexpress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstDepartureTime",
                table: "TransitRoute");

            migrationBuilder.DropColumn(
                name: "FrequencyMinutes",
                table: "TransitRoute");

            migrationBuilder.DropColumn(
                name: "LastDepartureTime",
                table: "TransitRoute");

            migrationBuilder.CreateTable(
                name: "RouteSchedules",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteId = table.Column<int>(type: "int", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_RouteSchedules_RouteId",
                table: "RouteSchedules",
                column: "RouteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RouteSchedules");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "FirstDepartureTime",
                table: "TransitRoute",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "FrequencyMinutes",
                table: "TransitRoute",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "LastDepartureTime",
                table: "TransitRoute",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
