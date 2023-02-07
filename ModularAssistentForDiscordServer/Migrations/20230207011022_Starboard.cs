using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MADS.Migrations
{
    /// <inheritdoc />
    public partial class Starboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "starboardChannel",
                table: "GuildConfigDbEntity",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "starboardEmojiId",
                table: "GuildConfigDbEntity",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "starboardEnabled",
                table: "GuildConfigDbEntity",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "starboardThreshold",
                table: "GuildConfigDbEntity",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Starboard",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    discordMessageId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    discordChannelId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    discordGuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    starCount = table.Column<int>(type: "int", nullable: false),
                    starboardMessageId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    starboardChannelId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    starboardGuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Starboard", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Starboard");

            migrationBuilder.DropColumn(
                name: "starboardChannel",
                table: "GuildConfigDbEntity");

            migrationBuilder.DropColumn(
                name: "starboardEmojiId",
                table: "GuildConfigDbEntity");

            migrationBuilder.DropColumn(
                name: "starboardEnabled",
                table: "GuildConfigDbEntity");

            migrationBuilder.DropColumn(
                name: "starboardThreshold",
                table: "GuildConfigDbEntity");
        }
    }
}
