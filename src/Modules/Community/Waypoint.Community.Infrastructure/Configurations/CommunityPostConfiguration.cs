using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.Community.Domain;

namespace Waypoint.Community.Infrastructure.Configurations;

public sealed class CommunityPostConfiguration : IEntityTypeConfiguration<CommunityPost>
{
    public void Configure(EntityTypeBuilder<CommunityPost> builder)
    {
        builder.ToTable("community_posts");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Body).HasMaxLength(2000).IsRequired();
        builder.Property(p => p.Visibility).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.Visibility);
        builder.UseXminConcurrencyToken();
    }
}
