using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Waypoint.Actions.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "actions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dream_id = table.Column<Guid>(type: "uuid", nullable: false),
                    goal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    mission_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    priority = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    difficulty = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    estimated_minutes = table.Column<int>(type: "integer", nullable: true),
                    expected_impact = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_next_best_action = table.Column<bool>(type: "boolean", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_actions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_actions_dream_id_status",
                table: "actions",
                columns: new[] { "dream_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ux_actions_one_next_best_per_dream",
                table: "actions",
                column: "dream_id",
                unique: true,
                filter: "is_next_best_action AND deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "actions");
        }
    }
}
