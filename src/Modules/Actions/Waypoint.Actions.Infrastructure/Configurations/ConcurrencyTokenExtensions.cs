using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Waypoint.Actions.Infrastructure.Configurations;

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
