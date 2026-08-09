using Waypoint.Audit.Domain;

namespace Waypoint.Audit.Application;

public interface IAuditLogRepository
{
    /// <summary>Most recent entries first, capped at <paramref name="take"/> — no pagination
    /// infrastructure exists anywhere else in the codebase yet, so this stays consistent with
    /// every other admin-facing list endpoint rather than introducing a one-off pattern here.</summary>
    Task<IReadOnlyList<AuditLogEntry>> GetRecentAsync(int take, CancellationToken cancellationToken);
}
