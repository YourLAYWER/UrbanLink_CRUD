using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanLink_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class UpdateJourneyLeg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fare",
                table: "RouteSegments");

            migrationBuilder.AddColumn<decimal>(
                name: "BaseFare",
                table: "TransitRoute",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseFare",
                table: "TransitRoute");

            migrationBuilder.AddColumn<decimal>(
                name: "Fare",
                table: "RouteSegments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
