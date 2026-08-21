using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Waypoint.Notifications.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPushNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notifications_delivery_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reminder_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    attempted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    http_status_code = table.Column<int>(type: "integer", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications_delivery_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notifications_push_subscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    endpoint = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    p256dh_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    auth_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_success_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_failure_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    consecutive_failure_count = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    deactivated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deactivated_reason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications_push_subscriptions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_delivery_history_status_attempted_at",
                table: "notifications_delivery_history",
                columns: new[] { "status", "attempted_at" });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_delivery_history_user_id_reminder_key",
                table: "notifications_delivery_history",
                columns: new[] { "user_id", "reminder_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_push_subscriptions_endpoint",
                table: "notifications_push_subscriptions",
                column: "endpoint",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_push_subscriptions_user_id_status",
                table: "notifications_push_subscriptions",
                columns: new[] { "user_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notifications_delivery_history");

            migrationBuilder.DropTable(
                name: "notifications_push_subscriptions");
        }
    }
}
