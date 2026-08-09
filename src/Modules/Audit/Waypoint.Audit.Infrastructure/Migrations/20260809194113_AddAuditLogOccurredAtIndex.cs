using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Waypoint.Audit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogOccurredAtIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_audit_log_occurred_at",
                table: "audit_log",
                column: "occurred_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_audit_log_occurred_at",
                table: "audit_log");
        }
    }
}
