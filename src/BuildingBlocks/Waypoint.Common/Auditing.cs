namespace Waypoint.Common;

public sealed record AuditEntry(
    string EntityType,
    Guid EntityId,
    string Action,
    Guid? ActorUserId,
    string? PayloadRedacted,
    DateTimeOffset OccurredAt);

/// <summary>
/// Cross-cutting audit port (see docs/03-domain-model.md "Cross-cutting concerns").
/// Every module writes through this interface; only Waypoint.Audit.Infrastructure
/// implements it, so no module needs a reference to the Audit module's tables.
/// </summary>
public interface IAuditSink
{
    Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken);
}

/// <summary>Identifies who is making the current request, without any module depending on ASP.NET Core directly.</summary>
public interface ICurrentUserAccessor
{
    Guid? UserId { get; }
    string? Email { get; }
}

/// <summary>
/// Each module's Infrastructure layer implements this over its own
/// DbContext. The host resolves every registered migrator at startup
/// (see docs/04-database-design.md "Migration strategy") so one module can
/// ship a migration without coordinating with another module's history.
/// </summary>
public interface IStartupMigrator
{
    Task MigrateAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Cross-module read contract owned by the Dreams module — the concrete
/// example named in docs/03-domain-model.md "Module communication rules"
/// (rule 2). Goals and Actions depend on this interface only, never on
/// Dreams' Application layer or DbContext directly.
/// </summary>
public sealed record DreamSummary(
    Guid DreamId,
    Guid UserId,
    string Title,
    string Statement,
    string? Purpose,
    string? WhoItHelps,
    string? Problem,
    string? Outcome,
    string? Motivation,
    string? Impact,
    bool IsBusinessShaped);

public interface IDreamSummaryProvider
{
    Task<DreamSummary?> GetForUserAsync(Guid userId, CancellationToken cancellationToken);
}
