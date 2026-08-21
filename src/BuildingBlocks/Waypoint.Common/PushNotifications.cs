namespace Waypoint.Common;

/// <summary>
/// Cross-module read contract owned by the Actions module (P1 push notifications) — same pattern
/// as IActionsSummaryProvider. Exposes exactly the computed "next best action" (title + rationale),
/// the same thing GetNextBestActionQuery already returns to Dashboard/Dream Overview/Actions, so
/// the Notifications worker never develops its own definition of "what's next." Both this provider
/// and GetNextBestActionQueryHandler call the same NextBestActionSelector.SelectFrom(...) - see
/// Waypoint.Actions.Infrastructure/NextBestActionSummaryProvider.cs.
/// </summary>
public sealed record NextBestActionSummary(Guid ActionId, string Title, string Rationale);

public interface INextBestActionSummaryProvider
{
    Task<NextBestActionSummary?> GetForUserAsync(Guid userId, CancellationToken cancellationToken);
}

/// <summary>
/// Cross-module read contract owned by the Users module (P1 push notifications). The Notifications
/// worker needs, for every user who has opted into the daily push reminder: their timezone
/// (Profile.TimeZone - the single source of truth, never duplicated), their quiet hours, and
/// whether they've opted into detailed notification content. Profile and NotificationPreferences
/// are both Users-owned 1:1-with-user tables in the same DbContext, so this is answered with one
/// query there rather than the worker resolving each user's profile individually.
/// </summary>
public sealed record PushReminderCandidate(
    Guid UserId,
    string TimeZone,
    TimeOnly? QuietHoursStart,
    TimeOnly? QuietHoursEnd,
    bool DetailedContentEnabled);

public interface IPushReminderAudienceProvider
{
    /// <summary>Every user with PushEnabled AND PushDailyReminderEnabled both true. Bounded by
    /// nothing today - Drevia's realistic user count makes loading this whole list once per worker
    /// tick trivial; revisit with paging only if that stops being true.</summary>
    Task<IReadOnlyList<PushReminderCandidate>> GetDailyReminderCandidatesAsync(CancellationToken cancellationToken);
}
