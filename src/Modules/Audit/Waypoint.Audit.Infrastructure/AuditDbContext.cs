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
        });
    }
}
