using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.Community.Domain;

namespace Waypoint.Community.Infrastructure.Configurations;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Body).HasMaxLength(1000).IsRequired();
        builder.HasIndex(c => c.PostId);
        builder.UseXminConcurrencyToken();
    }
}
