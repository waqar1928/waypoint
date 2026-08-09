using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Waypoint.BusinessIdeas.Infrastructure.Configurations;

/// <summary>See the identical helper in Waypoint.Users.Infrastructure for why this exists (no built-in Npgsql helper).</summary>
internal static class ConcurrencyTokenExtensions
{
    public static void UseXminConcurrencyToken<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }
}
