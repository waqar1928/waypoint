using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Waypoint.Users.Infrastructure.Configurations;

/// <summary>
/// Npgsql's EF Core provider has no built-in UseXminAsConcurrencyToken
/// helper (removed/never shipped in 9.x) — this maps the shadow property
/// the documented way: bind it to Postgres's system "xmin" column, which
/// the server increments on every row update, and treat it as the EF Core
/// concurrency token. See docs/04-database-design.md "Concurrency & integrity".
/// </summary>
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
