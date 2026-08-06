using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanLink_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class AddTransitControllersAndEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RouteSegmentId",
                table: "RouteStops",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RouteStops_RouteSegmentId",
                table: "RouteStops",
                column: "RouteSegmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_RouteStops_RouteSegments_RouteSegmentId",
                table: "RouteStops",
                column: "RouteSegmentId",
                principalTable: "RouteSegments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RouteStops_RouteSegments_RouteSegmentId",
                table: "RouteStops");

            migrationBuilder.DropIndex(
                name: "IX_RouteStops_RouteSegmentId",
                table: "RouteStops");

            migrationBuilder.DropColumn(
                name: "RouteSegmentId",
                table: "RouteStops");
        }
    }
}
