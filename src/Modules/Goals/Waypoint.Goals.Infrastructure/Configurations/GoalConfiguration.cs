using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.Goals.Domain;

namespace Waypoint.Goals.Infrastructure.Configurations;

public sealed class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("goals");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Horizon).HasConversion<string>().HasMaxLength(20);
        builder.Property(g => g.Statement).HasMaxLength(1000).IsRequired();
        builder.HasIndex(g => g.DreamId);
        builder.UseXminConcurrencyToken();
    }
}
