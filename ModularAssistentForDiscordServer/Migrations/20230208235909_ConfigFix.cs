using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MADS.Migrations
{
    /// <inheritdoc />
    public partial class ConfigFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guilds_GuildConfigDbEntity_id",
                table: "Guilds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildConfigDbEntity",
                table: "GuildConfigDbEntity");

            migrationBuilder.RenameTable(
                name: "GuildConfigDbEntity",
                newName: "Configs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Configs",
                table: "Configs",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Guilds_Configs_id",
                table: "Guilds",
                column: "id",
                principalTable: "Configs",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guilds_Configs_id",
                table: "Guilds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Configs",
                table: "Configs");

            migrationBuilder.RenameTable(
                name: "Configs",
                newName: "GuildConfigDbEntity");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildConfigDbEntity",
                table: "GuildConfigDbEntity",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Guilds_GuildConfigDbEntity_id",
                table: "Guilds",
                column: "id",
                principalTable: "GuildConfigDbEntity",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
