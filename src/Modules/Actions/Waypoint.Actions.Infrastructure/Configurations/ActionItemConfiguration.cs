using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.Actions.Domain;

namespace Waypoint.Actions.Infrastructure.Configurations;

public sealed class ActionItemConfiguration : IEntityTypeConfiguration<ActionItem>
{
    public void Configure(EntityTypeBuilder<ActionItem> builder)
    {
        builder.ToTable("actions");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Title).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(2000);
        builder.Property(a => a.Priority).HasConversion<string>().HasMaxLength(10);
        builder.Property(a => a.Difficulty).HasConversion<string>().HasMaxLength(10);
        builder.Property(a => a.ExpectedImpact).HasConversion<string>().HasMaxLength(10);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(a => new { a.DreamId, a.Status });
        builder.HasIndex(a => a.DreamId)
            .IsUnique()
            .HasFilter("is_next_best_action AND deleted_at IS NULL")
            .HasDatabaseName("ux_actions_one_next_best_per_dream");

        builder.UseXminConcurrencyToken();
    }
}
