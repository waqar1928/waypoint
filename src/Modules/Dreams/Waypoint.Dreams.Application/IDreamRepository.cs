using Waypoint.Dreams.Domain;

namespace Waypoint.Dreams.Application;

public interface IDreamRepository
{
    Task<Dream?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> ExistsForUserAsync(Guid userId, CancellationToken cancellationToken);
    Task SaveAsync(Dream dream, CancellationToken cancellationToken);

    /// <summary>Real hard delete (not the DeletedAt soft-delete every normal read filters on) —
    /// backs account deletion (see docs/PRODUCTION_READINESS_AUDIT.md's Data Protection section).
    /// A soft-delete would leave the actual Dream Statement content sitting in the database
    /// forever, which defeats the point of deleting an account. DreamStatement cascades from Dream
    /// at the Postgres FK level (see DreamConfiguration), so deleting the Dream row is sufficient —
    /// no separate DreamStatement delete needed.</summary>
    Task DeleteForUserAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Admin-only (Phase 8 oversight view) — every dream across every user, most recent
    /// first, capped at <paramref name="take"/> as a Phase 10 safety net against unbounded growth.</summary>
    Task<IReadOnlyList<Dream>> GetAllAsync(int take, CancellationToken cancellationToken);
}
