using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanLink_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAndEntityIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Users_UserId",
                table: "Cards");

            migrationBuilder.DropForeignKey(
                name: "FK_TransitCards_Cards_CardId",
                table: "TransitCards");

            migrationBuilder.DropForeignKey(
                name: "FK_TransitCards_TransitProviders_ProviderId",
                table: "TransitCards");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProviders_TransitProviders_ProviderId",
                table: "UserProviders");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProviders_Users_UserId",
                table: "UserProviders");

            migrationBuilder.DropIndex(
                name: "IX_UserProviders_UserId",
                table: "UserProviders");

            migrationBuilder.DropIndex(
                name: "IX_ConductorShifts_ConductorId",
                table: "ConductorShifts");

            migrationBuilder.DropIndex(
                name: "IX_Cards_UserId",
                table: "Cards");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "OperatorCardNumber",
                table: "TransitCards",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Journeys",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role_IsActive",
                table: "Users",
                columns: new[] { "Role", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_UserProviders_UserId_ProviderId",
                table: "UserProviders",
                columns: new[] { "UserId", "ProviderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransitCards_OperatorCardNumber",
                table: "TransitCards",
                column: "OperatorCardNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Journeys_Status_EndStopID",
                table: "Journeys",
                columns: new[] { "Status", "EndStopID" });

            migrationBuilder.CreateIndex(
                name: "IX_Journeys_TicketReference",
                table: "Journeys",
                column: "TicketReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConductorShifts_ConductorId_ShiftEndTime",
                table: "ConductorShifts",
                columns: new[] { "ConductorId", "ShiftEndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_User_Default_Active",
                table: "Cards",
                columns: new[] { "UserId", "IsDefault", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_UserId_IsDefault_IsActive",
                table: "Cards",
                columns: new[] { "UserId", "IsDefault", "IsActive" });

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Users_UserId",
                table: "Cards",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransitCards_Cards_CardId",
                table: "TransitCards",
                column: "CardId",
                principalTable: "Cards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransitCards_TransitProviders_ProviderId",
                table: "TransitCards",
                column: "ProviderId",
                principalTable: "TransitProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProviders_TransitProviders_ProviderId",
                table: "UserProviders",
                column: "ProviderId",
                principalTable: "TransitProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProviders_Users_UserId",
                table: "UserProviders",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Users_UserId",
                table: "Cards");

            migrationBuilder.DropForeignKey(
                name: "FK_TransitCards_Cards_CardId",
                table: "TransitCards");

            migrationBuilder.DropForeignKey(
                name: "FK_TransitCards_TransitProviders_ProviderId",
                table: "TransitCards");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProviders_TransitProviders_ProviderId",
                table: "UserProviders");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProviders_Users_UserId",
                table: "UserProviders");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Role_IsActive",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserProviders_UserId_ProviderId",
                table: "UserProviders");

            migrationBuilder.DropIndex(
                name: "IX_TransitCards_OperatorCardNumber",
                table: "TransitCards");

            migrationBuilder.DropIndex(
                name: "IX_Journeys_Status_EndStopID",
                table: "Journeys");

            migrationBuilder.DropIndex(
                name: "IX_Journeys_TicketReference",
                table: "Journeys");

            migrationBuilder.DropIndex(
                name: "IX_ConductorShifts_ConductorId_ShiftEndTime",
                table: "ConductorShifts");

            migrationBuilder.DropIndex(
                name: "IX_Cards_User_Default_Active",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_Cards_UserId_IsDefault_IsActive",
                table: "Cards");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "OperatorCardNumber",
                table: "TransitCards",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Journeys",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_UserProviders_UserId",
                table: "UserProviders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConductorShifts_ConductorId",
                table: "ConductorShifts",
                column: "ConductorId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_UserId",
                table: "Cards",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Users_UserId",
                table: "Cards",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransitCards_Cards_CardId",
                table: "TransitCards",
                column: "CardId",
                principalTable: "Cards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransitCards_TransitProviders_ProviderId",
                table: "TransitCards",
                column: "ProviderId",
                principalTable: "TransitProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProviders_TransitProviders_ProviderId",
                table: "UserProviders",
                column: "ProviderId",
                principalTable: "TransitProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProviders_Users_UserId",
                table: "UserProviders",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
