using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.Dreams.Domain;

namespace Waypoint.Dreams.Infrastructure.Configurations;

public sealed class DreamStatementConfiguration : IEntityTypeConfiguration<DreamStatement>
{
    public void Configure(EntityTypeBuilder<DreamStatement> builder)
    {
        builder.ToTable("dream_statements");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Statement).HasMaxLength(2000).IsRequired();
        builder.Property(s => s.Purpose).HasMaxLength(2000);
        builder.Property(s => s.WhoItHelps).HasMaxLength(2000);
        builder.Property(s => s.Problem).HasMaxLength(2000);
        builder.Property(s => s.Outcome).HasMaxLength(2000);
        builder.Property(s => s.Motivation).HasMaxLength(2000);
        builder.Property(s => s.Impact).HasMaxLength(2000);
        builder.HasIndex(s => s.DreamId).IsUnique();
    }
}
