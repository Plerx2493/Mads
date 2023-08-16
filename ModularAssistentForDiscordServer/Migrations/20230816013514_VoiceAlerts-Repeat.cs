using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MADS.Migrations
{
    /// <inheritdoc />
    public partial class VoiceAlertsRepeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_repeatable",
                table: "VoiceAlerts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_repeatable",
                table: "VoiceAlerts");
        }
    }
}
