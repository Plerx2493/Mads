using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MADS.Migrations
{
    /// <inheritdoc />
    public partial class Reminder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    userId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    reminderText = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    executionTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    isPrivate = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    channelId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    messageId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    mentionedChannel = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    MentionedMessage = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.id);
                    table.ForeignKey(
                        name: "FK_Reminders_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_target_id",
                table: "Incidents",
                column: "target_id");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_userId",
                table: "Reminders",
                column: "userId");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Users_target_id",
                table: "Incidents",
                column: "target_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Users_target_id",
                table: "Incidents");

            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_target_id",
                table: "Incidents");

            migrationBuilder.AddColumn<ulong>(
                name: "TargetUserId",
                table: "Incidents",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_TargetUserId",
                table: "Incidents",
                column: "TargetUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Users_TargetUserId",
                table: "Incidents",
                column: "TargetUserId",
                principalTable: "Users",
                principalColumn: "id");
        }
    }
}
