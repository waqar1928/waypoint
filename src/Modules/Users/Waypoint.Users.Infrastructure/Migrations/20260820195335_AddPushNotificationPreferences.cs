using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Waypoint.Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPushNotificationPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "push_daily_reminder_enabled",
                table: "users_notification_preferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "push_detailed_content",
                table: "users_notification_preferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "push_enabled",
                table: "users_notification_preferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "quiet_hours_end",
                table: "users_notification_preferences",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "quiet_hours_start",
                table: "users_notification_preferences",
                type: "time without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "push_daily_reminder_enabled",
                table: "users_notification_preferences");

            migrationBuilder.DropColumn(
                name: "push_detailed_content",
                table: "users_notification_preferences");

            migrationBuilder.DropColumn(
                name: "push_enabled",
                table: "users_notification_preferences");

            migrationBuilder.DropColumn(
                name: "quiet_hours_end",
                table: "users_notification_preferences");

            migrationBuilder.DropColumn(
                name: "quiet_hours_start",
                table: "users_notification_preferences");
        }
    }
}
