using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.Goals.Domain;

namespace Waypoint.Goals.Infrastructure.Configurations;

public sealed class MissionConfiguration : IEntityTypeConfiguration<Mission>
{
    public void Configure(EntityTypeBuilder<Mission> builder)
    {
        builder.ToTable("missions");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Title).HasMaxLength(200).IsRequired();
        builder.HasIndex(m => m.GoalId);
        builder.UseXminConcurrencyToken();
    }
}
