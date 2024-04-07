using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MADS.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserDeletionConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Users_target_id",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_target_id",
                table: "Incidents");

            migrationBuilder.AddColumn<ulong>(
                name: "TargetUserId",
                table: "Incidents",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_TargetUserId",
                table: "Incidents",
                column: "TargetUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Users_TargetUserId",
                table: "Incidents",
                column: "TargetUserId",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Users_TargetUserId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_TargetUserId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "TargetUserId",
                table: "Incidents");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_target_id",
                table: "Incidents",
                column: "target_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Users_target_id",
                table: "Incidents",
                column: "target_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
