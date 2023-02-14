using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MADS.Migrations
{
    /// <inheritdoc />
    public partial class Fixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guilds_Configs_id",
                table: "Guilds");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Guilds_Id",
                table: "Incidents");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Incidents",
                newName: "id");

            migrationBuilder.AlterColumn<ulong>(
                name: "id",
                table: "Incidents",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<ulong>(
                name: "guild_id",
                table: "Incidents",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AlterColumn<ulong>(
                name: "id",
                table: "Guilds",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Guilds_discordId",
                table: "Guilds",
                column: "discordId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Configs_discordId",
                table: "Configs",
                column: "discordId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_guild_id",
                table: "Incidents",
                column: "guild_id");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guilds_Configs_discordId",
                table: "Guilds");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Guilds_guild_id",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_guild_id",
                table: "Incidents");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Guilds_discordId",
                table: "Guilds");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Configs_discordId",
                table: "Configs");

            migrationBuilder.DropColumn(
                name: "guild_id",
                table: "Incidents");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Incidents",
                newName: "Id");

            migrationBuilder.AlterColumn<ulong>(
                name: "Id",
                table: "Incidents",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<ulong>(
                name: "id",
                table: "Guilds",
                type: "bigint unsigned",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddForeignKey(
                name: "FK_Guilds_Configs_id",
                table: "Guilds",
                column: "id",
                principalTable: "Configs",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Guilds_Id",
                table: "Incidents",
                column: "Id",
                principalTable: "Guilds",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
