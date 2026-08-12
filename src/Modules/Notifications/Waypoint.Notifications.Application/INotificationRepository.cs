using Waypoint.Notifications.Domain;

namespace Waypoint.Notifications.Application;

public interface INotificationRepository
{
    /// <summary>Most recent first, capped at <paramref name="take"/> — same safety-net pattern as
    /// every other list read in this codebase (see docs/PRODUCTION_READINESS_AUDIT.md Performance
    /// section on Phase 10's pagination sweep).</summary>
    Task<IReadOnlyList<Notification>> GetForUserAsync(Guid userId, int take, CancellationToken cancellationToken);

    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Tracked, single-row lookup — deliberately not AsNoTracking, since MarkAsRead
    /// loads-then-mutates-then-saves this exact instance (see the Phase 10 xmin-concurrency
    /// lesson documented on GoalsRepository.GetMilestoneByIdAsync; this repository follows the
    /// same rule from day one rather than repeating that mistake).</summary>
    Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken);

    Task AddAsync(Notification notification, CancellationToken cancellationToken);

    Task SaveAsync(Notification notification, CancellationToken cancellationToken);

    /// <summary>Bulk mark-all-read — done as a single set-based update rather than loading every
    /// unread row into memory, since a user could plausibly have hundreds of unread notifications.</summary>
    Task MarkAllReadAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Real hard delete of every notification addressed to this user — backs account
    /// deletion (see docs/PRODUCTION_READINESS_AUDIT.md's Data Protection section). Deliberately
    /// only targets RecipientUserId, not notifications this user's own actions caused to be sent
    /// to *other* people (e.g. a comment they made that notified a post's author) — those
    /// notifications belong to the recipient, not the deleted user, and their content was already
    /// snapshotted at send time rather than referencing this user live.</summary>
    Task DeleteAllForUserAsync(Guid userId, CancellationToken cancellationToken);
}
