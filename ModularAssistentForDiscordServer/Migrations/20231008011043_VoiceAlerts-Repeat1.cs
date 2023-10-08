using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MADS.Migrations
{
    /// <inheritdoc />
    public partial class VoiceAlertsRepeat1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_alert",
                table: "VoiceAlerts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "time_between",
                table: "VoiceAlerts",
                type: "time(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_alert",
                table: "VoiceAlerts");

            migrationBuilder.DropColumn(
                name: "time_between",
                table: "VoiceAlerts");
        }
    }
}
