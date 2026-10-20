using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanLink_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketRouteAndPurchasedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PurchasedAt",
                table: "Tickets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "RouteId",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_RouteId",
                table: "Tickets",
                column: "RouteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_TransitRoute_RouteId",
                table: "Tickets",
                column: "RouteId",
                principalTable: "TransitRoute",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_TransitRoute_RouteId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_RouteId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "PurchasedAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "RouteId",
                table: "Tickets");
        }
    }
}
