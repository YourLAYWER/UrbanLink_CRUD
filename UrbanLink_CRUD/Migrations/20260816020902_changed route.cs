using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanLink_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class changedroute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
