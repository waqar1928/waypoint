using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.Experiments.Domain;

namespace Waypoint.Experiments.Infrastructure.Configurations;

public sealed class ExperimentResultConfiguration : IEntityTypeConfiguration<ExperimentResult>
{
    public void Configure(EntityTypeBuilder<ExperimentResult> builder)
    {
        builder.ToTable("experiment_results");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Outcome).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Evidence).HasMaxLength(2000);
        builder.Property(r => r.Learning).HasMaxLength(2000).IsRequired();
        builder.HasIndex(r => r.ExperimentId);
        builder.UseXminConcurrencyToken();
    }
}
