using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanLink_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class secondMigration35 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ConductorShifts_ConductorId",
                table: "ConductorShifts",
                column: "ConductorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConductorShifts_Users_ConductorId",
                table: "ConductorShifts",
                column: "ConductorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConductorShifts_Users_ConductorId",
                table: "ConductorShifts");

            migrationBuilder.DropIndex(
                name: "IX_ConductorShifts_ConductorId",
                table: "ConductorShifts");
        }
    }
}
