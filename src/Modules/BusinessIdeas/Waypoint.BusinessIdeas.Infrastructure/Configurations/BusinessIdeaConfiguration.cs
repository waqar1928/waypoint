using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.BusinessIdeas.Domain;

namespace Waypoint.BusinessIdeas.Infrastructure.Configurations;

public sealed class BusinessIdeaConfiguration : IEntityTypeConfiguration<BusinessIdea>
{
    public void Configure(EntityTypeBuilder<BusinessIdea> builder)
    {
        builder.ToTable("business_ideas");
        builder.HasKey(i => i.Id);

        // Free text, unbounded (docs/04-database-design.md sketches these as bare `text` columns,
        // not varchar(n) — no MaxLength here is intentional, matches the DDL).
        builder.HasIndex(i => i.DreamId).IsUnique();

        builder.UseXminConcurrencyToken();
    }
}
