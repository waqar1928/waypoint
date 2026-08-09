using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.Experiments.Domain;

namespace Waypoint.Experiments.Infrastructure.Configurations;

public sealed class ExperimentConfiguration : IEntityTypeConfiguration<Experiment>
{
    public void Configure(EntityTypeBuilder<Experiment> builder)
    {
        builder.ToTable("experiments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.IdeaDescription).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Hypothesis).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.SuccessCriteria).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(e => new { e.DreamId, e.Status });
        builder.UseXminConcurrencyToken();
    }
}
