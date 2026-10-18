using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanLink_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class NormalizedJourney : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Journeys_ConductorShifts_IssuedDuringShiftId",
                table: "Journeys");

            migrationBuilder.DropForeignKey(
                name: "FK_Journeys_Stops_EndStopId",
                table: "Journeys");

            migrationBuilder.DropForeignKey(
                name: "FK_Journeys_Stops_StartStopId",
                table: "Journeys");

            migrationBuilder.DropForeignKey(
                name: "FK_Journeys_Users_UserId",
                table: "Journeys");

            migrationBuilder.DropIndex(
                name: "IX_Journeys_IssuedDuringShiftId",
                table: "Journeys");

            migrationBuilder.DropIndex(
                name: "IX_Journeys_StartStopId",
                table: "Journeys");

            migrationBuilder.DropIndex(
                name: "IX_Journeys_UserId",
                table: "Journeys");

            migrationBuilder.DropColumn(
                name: "IssuedDuringShiftId",
                table: "Journeys");

            migrationBuilder.DropColumn(
                name: "StartStopId",
                table: "Journeys");

            migrationBuilder.DropColumn(
                name: "TotalDurationMinutes",
                table: "Journeys");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Journeys");

            migrationBuilder.RenameColumn(
                name: "EndStopId",
                table: "Journeys",
                newName: "EndStopID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Journeys",
                newName: "JourneyID");

            migrationBuilder.RenameIndex(
                name: "IX_Journeys_EndStopId",
                table: "Journeys",
                newName: "IX_Journeys_EndStopID");

            migrationBuilder.AlterColumn<int>(
                name: "TransportType",
                table: "TransitRoute",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "TransitProviders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "EndStopID",
                table: "Journeys",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "JourneyLegs",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "IncidentReport",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Category",
                table: "IncidentReport",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "ConductorId",
                table: "ConductorShifts",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "CashJourneys",
                columns: table => new
                {
                    JourneyID = table.Column<int>(type: "int", nullable: false),
                    ShiftID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashJourneys", x => x.JourneyID);
                    table.ForeignKey(
                        name: "FK_CashJourneys_ConductorShifts_ShiftID",
                        column: x => x.ShiftID,
                        principalTable: "ConductorShifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashJourneys_Journeys_JourneyID",
                        column: x => x.JourneyID,
                        principalTable: "Journeys",
                        principalColumn: "JourneyID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserJourneys",
                columns: table => new
                {
                    JourneyID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    StartStopID = table.Column<int>(type: "int", nullable: false),
                    EstimatedArrivalTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserJourneys", x => x.JourneyID);
                    table.ForeignKey(
                        name: "FK_UserJourneys_Journeys_JourneyID",
                        column: x => x.JourneyID,
                        principalTable: "Journeys",
                        principalColumn: "JourneyID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserJourneys_Stops_StartStopID",
                        column: x => x.StartStopID,
                        principalTable: "Stops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserJourneys_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashJourneys_ShiftID",
                table: "CashJourneys",
                column: "ShiftID");

            migrationBuilder.CreateIndex(
                name: "IX_UserJourneys_StartStopID",
                table: "UserJourneys",
                column: "StartStopID");

            migrationBuilder.CreateIndex(
                name: "IX_UserJourneys_UserID",
                table: "UserJourneys",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Journeys_Stops_EndStopID",
                table: "Journeys",
                column: "EndStopID",
                principalTable: "Stops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Journeys_Stops_EndStopID",
                table: "Journeys");

            migrationBuilder.DropTable(
                name: "CashJourneys");

            migrationBuilder.DropTable(
                name: "UserJourneys");

            migrationBuilder.RenameColumn(
                name: "EndStopID",
                table: "Journeys",
                newName: "EndStopId");

            migrationBuilder.RenameColumn(
                name: "JourneyID",
                table: "Journeys",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Journeys_EndStopID",
                table: "Journeys",
                newName: "IX_Journeys_EndStopId");

            migrationBuilder.AlterColumn<string>(
                name: "TransportType",
                table: "TransitRoute",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "TransitProviders",
                type: "nvarchar(max)",
                nullable: false,
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
                name: "StartStopId",
                table: "Journeys",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalDurationMinutes",
                table: "Journeys",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Journeys",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "JourneyLegs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "IncidentReport",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "IncidentReport",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ConductorId",
                table: "ConductorShifts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Journeys_IssuedDuringShiftId",
                table: "Journeys",
                column: "IssuedDuringShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_Journeys_StartStopId",
                table: "Journeys",
                column: "StartStopId");

            migrationBuilder.CreateIndex(
                name: "IX_Journeys_UserId",
                table: "Journeys",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Journeys_ConductorShifts_IssuedDuringShiftId",
                table: "Journeys",
                column: "IssuedDuringShiftId",
                principalTable: "ConductorShifts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Journeys_Stops_EndStopId",
                table: "Journeys",
                column: "EndStopId",
                principalTable: "Stops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Journeys_Stops_StartStopId",
                table: "Journeys",
                column: "StartStopId",
                principalTable: "Stops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Journeys_Users_UserId",
                table: "Journeys",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
