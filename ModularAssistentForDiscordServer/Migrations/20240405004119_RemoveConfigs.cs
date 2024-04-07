using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MADS.Migrations
{
    /// <inheritdoc />
    public partial class RemoveConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guilds_Configs_discordId",
                table: "Guilds");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Guilds_guild_id",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_Guilds_discordGuildId",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_discordGuildId",
                table: "Quotes");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Guilds_discordId",
                table: "Guilds");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Configs_discordId",
                table: "Configs");

            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "Quotes",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "SettingsId",
                table: "Guilds",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_GuildId",
                table: "Quotes",
                column: "GuildId");

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
                name: "FK_Quotes_Guilds_GuildId",
                table: "Quotes",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guilds_Configs_SettingsId",
                table: "Guilds");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Guilds_guild_id",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_Guilds_GuildId",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_GuildId",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_Guilds_SettingsId",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "SettingsId",
                table: "Guilds");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Guilds_discordId",
                table: "Guilds",
                column: "discordId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Configs_discordId",
                table: "Configs",
                column: "discordId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_discordGuildId",
                table: "Quotes",
                column: "discordGuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_Guilds_Configs_discordId",
                table: "Guilds",
                column: "discordId",
                principalTable: "Configs",
                principalColumn: "discordId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Guilds_guild_id",
                table: "Incidents",
                column: "guild_id",
                principalTable: "Guilds",
                principalColumn: "discordId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_Guilds_discordGuildId",
                table: "Quotes",
                column: "discordGuildId",
                principalTable: "Guilds",
                principalColumn: "discordId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
