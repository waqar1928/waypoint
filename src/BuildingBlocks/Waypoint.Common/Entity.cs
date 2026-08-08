namespace Waypoint.Common;

/// <summary>
/// Base type for every aggregate/entity across every module. Carries the
/// standard envelope (see docs/03-domain-model.md) so no module has to
/// re-derive audit/concurrency/multi-tenancy plumbing.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid? TenantId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }

    // Optimistic concurrency is handled per-entity in each module's EF Core configuration via
    // Npgsql's UseXminAsConcurrencyToken() (see docs/04-database-design.md "Concurrency & integrity"),
    // not a physical property here — Postgres already tracks this via the system xmin column.
}

public interface ISoftDeletable
{
    DateTimeOffset? DeletedAt { get; set; }
}
