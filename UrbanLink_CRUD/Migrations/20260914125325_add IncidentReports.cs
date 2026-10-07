using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanLink_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class addIncidentReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IncidentReport_ConductorShifts_ConductorShiftId",
                table: "IncidentReport");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IncidentReport",
                table: "IncidentReport");

            migrationBuilder.RenameTable(
                name: "IncidentReport",
                newName: "IncidentReports");

            migrationBuilder.RenameIndex(
                name: "IX_IncidentReport_ConductorShiftId",
                table: "IncidentReports",
                newName: "IX_IncidentReports_ConductorShiftId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IncidentReports",
                table: "IncidentReports",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentReports_ConductorShifts_ConductorShiftId",
                table: "IncidentReports",
                column: "ConductorShiftId",
                principalTable: "ConductorShifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IncidentReports_ConductorShifts_ConductorShiftId",
                table: "IncidentReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IncidentReports",
                table: "IncidentReports");

            migrationBuilder.RenameTable(
                name: "IncidentReports",
                newName: "IncidentReport");

            migrationBuilder.RenameIndex(
                name: "IX_IncidentReports_ConductorShiftId",
                table: "IncidentReport",
                newName: "IX_IncidentReport_ConductorShiftId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IncidentReport",
                table: "IncidentReport",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentReport_ConductorShifts_ConductorShiftId",
                table: "IncidentReport",
                column: "ConductorShiftId",
                principalTable: "ConductorShifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
