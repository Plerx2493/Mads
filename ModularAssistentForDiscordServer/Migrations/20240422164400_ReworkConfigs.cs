using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MADS.Migrations
{
    /// <inheritdoc />
    public partial class ReworkConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guilds_Configs_SettingsId",
                table: "Guilds");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Guilds_guild_id",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Users_TargetUserId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_Guilds_GuildId",
                table: "Quotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Reminders_Users_userId",
                table: "Reminders");

            migrationBuilder.DropForeignKey(
                name: "FK_VoiceAlerts_Users_user_id",
                table: "VoiceAlerts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_GuildId",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_TargetUserId",
                table: "Incidents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Guilds",
                table: "Guilds");

            migrationBuilder.DropIndex(
                name: "IX_Guilds_SettingsId",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "discriminator",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "TargetUserId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "SettingsId",
                table: "Guilds");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Guilds",
                newName: "guilds");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_guilds",
                table: "guilds",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_discordGuildId",
                table: "Quotes",
                column: "discordGuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_target_id",
                table: "Incidents",
                column: "target_id");

            migrationBuilder.CreateIndex(
                name: "IX_Configs_discordId",
                table: "Configs",
                column: "discordId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Configs_guilds_discordId",
                table: "Configs",
                column: "discordId",
                principalTable: "guilds",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_guilds_guild_id",
                table: "Incidents",
                column: "guild_id",
                principalTable: "guilds",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_users_target_id",
                table: "Incidents",
                column: "target_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_guilds_discordGuildId",
                table: "Quotes",
                column: "discordGuildId",
                principalTable: "guilds",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reminders_users_userId",
                table: "Reminders",
                column: "userId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VoiceAlerts_users_user_id",
                table: "VoiceAlerts",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Configs_guilds_discordId",
                table: "Configs");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_guilds_guild_id",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_users_target_id",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_guilds_discordGuildId",
                table: "Quotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Reminders_users_userId",
                table: "Reminders");

            migrationBuilder.DropForeignKey(
                name: "FK_VoiceAlerts_users_user_id",
                table: "VoiceAlerts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_discordGuildId",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_target_id",
                table: "Incidents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_guilds",
                table: "guilds");

            migrationBuilder.DropIndex(
                name: "IX_Configs_discordId",
                table: "Configs");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "guilds",
                newName: "Guilds");

            migrationBuilder.AddColumn<int>(
                name: "discriminator",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "Quotes",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "TargetUserId",
                table: "Incidents",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "SettingsId",
                table: "Guilds",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Guilds",
                table: "Guilds",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_GuildId",
                table: "Quotes",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_TargetUserId",
                table: "Incidents",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Guilds_SettingsId",
                table: "Guilds",
                column: "SettingsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Guilds_Configs_SettingsId",
                table: "Guilds",
                column: "SettingsId",
                principalTable: "Configs",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Guilds_guild_id",
                table: "Incidents",
                column: "guild_id",
                principalTable: "Guilds",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Users_TargetUserId",
                table: "Incidents",
                column: "TargetUserId",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_Guilds_GuildId",
                table: "Quotes",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reminders_Users_userId",
                table: "Reminders",
                column: "userId",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VoiceAlerts_Users_user_id",
                table: "VoiceAlerts",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
