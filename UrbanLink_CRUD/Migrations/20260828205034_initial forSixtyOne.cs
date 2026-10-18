using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanLink_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class initialforSixtyOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusId1",
                table: "ConductorShifts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConductorShifts_BusId1",
                table: "ConductorShifts",
                column: "BusId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ConductorShifts_Buses_BusId1",
                table: "ConductorShifts",
                column: "BusId1",
                principalTable: "Buses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConductorShifts_Buses_BusId1",
                table: "ConductorShifts");

            migrationBuilder.DropIndex(
                name: "IX_ConductorShifts_BusId1",
                table: "ConductorShifts");

            migrationBuilder.DropColumn(
                name: "BusId1",
                table: "ConductorShifts");
        }
    }
}
