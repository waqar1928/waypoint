using Microsoft.EntityFrameworkCore;
using Waypoint.Audit.Domain;

namespace Waypoint.Audit.Infrastructure;

public sealed class AuditDbContext(DbContextOptions<AuditDbContext> options) : DbContext(options)
{
    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLogEntry>(entry =>
        {
            entry.ToTable("audit_log");
            entry.HasKey(e => e.Id);
            entry.Property(e => e.EntityType).HasMaxLength(100).IsRequired();
            entry.Property(e => e.Action).HasMaxLength(50).IsRequired();
            entry.HasIndex(e => new { e.EntityType, e.EntityId });
            entry.HasIndex(e => new { e.ActorUserId, e.OccurredAt });

            // GetRecentAsync (the admin audit-log feed) does `ORDER BY occurred_at DESC LIMIT n`
            // with no WHERE clause — neither composite index above has OccurredAt as its leading
            // column, so as the log grows this query would degrade into a full-table sort. A
            // standalone index on OccurredAt lets Postgres serve it as a cheap backward index scan.
            entry.HasIndex(e => e.OccurredAt);
        });
    }
}
