using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanLink_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class addedauditTrailonJourney : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Journeys_Users_UserId",
                table: "Journeys");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Journeys",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "StartStopId",
                table: "Journeys",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EndStopId",
                table: "Journeys",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "IssuedDuringShiftId",
                table: "Journeys",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DigitalTicketScanned",
                table: "ConductorShifts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Journeys_IssuedDuringShiftId",
                table: "Journeys",
                column: "IssuedDuringShiftId");

            migrationBuilder.AddForeignKey(
                name: "FK_Journeys_ConductorShifts_IssuedDuringShiftId",
                table: "Journeys",
                column: "IssuedDuringShiftId",
                principalTable: "ConductorShifts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Journeys_Users_UserId",
                table: "Journeys",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Journeys_ConductorShifts_IssuedDuringShiftId",
                table: "Journeys");

            migrationBuilder.DropForeignKey(
                name: "FK_Journeys_Users_UserId",
                table: "Journeys");

            migrationBuilder.DropIndex(
                name: "IX_Journeys_IssuedDuringShiftId",
                table: "Journeys");

            migrationBuilder.DropColumn(
                name: "IssuedDuringShiftId",
                table: "Journeys");

            migrationBuilder.DropColumn(
                name: "DigitalTicketScanned",
                table: "ConductorShifts");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Journeys",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "StartStopId",
                table: "Journeys",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EndStopId",
                table: "Journeys",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Journeys_Users_UserId",
                table: "Journeys",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
