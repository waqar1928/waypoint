using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Waypoint.Community.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentReportStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "content_reports",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_content_reports_status",
                table: "content_reports",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_content_reports_status",
                table: "content_reports");

            migrationBuilder.DropColumn(
                name: "status",
                table: "content_reports");
        }
    }
}
