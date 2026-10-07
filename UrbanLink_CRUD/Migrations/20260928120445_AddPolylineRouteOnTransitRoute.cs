using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanLink_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class AddPolylineRouteOnTransitRoute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RouteMapPath",
                table: "Stops");

            migrationBuilder.AddColumn<string>(
                name: "RouteMapPath",
                table: "TransitRoute",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RouteMapPath",
                table: "TransitRoute");

            migrationBuilder.AddColumn<string>(
                name: "RouteMapPath",
                table: "Stops",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
