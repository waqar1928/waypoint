using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Waypoint.BusinessIdeas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "business_ideas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dream_id = table.Column<Guid>(type: "uuid", nullable: false),
                    problem = table.Column<string>(type: "text", nullable: true),
                    customer = table.Column<string>(type: "text", nullable: true),
                    value_proposition = table.Column<string>(type: "text", nullable: true),
                    solution = table.Column<string>(type: "text", nullable: true),
                    business_model = table.Column<string>(type: "text", nullable: true),
                    market = table.Column<string>(type: "text", nullable: true),
                    competitors = table.Column<string>(type: "text", nullable: true),
                    pricing = table.Column<string>(type: "text", nullable: true),
                    marketing = table.Column<string>(type: "text", nullable: true),
                    sales = table.Column<string>(type: "text", nullable: true),
                    operations = table.Column<string>(type: "text", nullable: true),
                    technology = table.Column<string>(type: "text", nullable: true),
                    financial_assumptions = table.Column<string>(type: "text", nullable: true),
                    risks = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_business_ideas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "business_validations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    business_idea_id = table.Column<Guid>(type: "uuid", nullable: false),
                    viability_estimate = table.Column<int>(type: "integer", nullable: true),
                    strong_assumptions = table.Column<string>(type: "jsonb", nullable: false),
                    weak_assumptions = table.Column<string>(type: "jsonb", nullable: false),
                    unknowns = table.Column<string>(type: "jsonb", nullable: false),
                    recommended_experiments = table.Column<string>(type: "jsonb", nullable: false),
                    generated_by_ai = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_business_validations", x => x.id);
                    table.CheckConstraint("ck_business_validations_viability_estimate_range", "viability_estimate BETWEEN 0 AND 100");
                });

            migrationBuilder.CreateIndex(
                name: "ix_business_ideas_dream_id",
                table: "business_ideas",
                column: "dream_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_business_validations_business_idea_id",
                table: "business_validations",
                column: "business_idea_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "business_ideas");

            migrationBuilder.DropTable(
                name: "business_validations");
        }
    }
}
