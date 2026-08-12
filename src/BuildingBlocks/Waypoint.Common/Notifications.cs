namespace Waypoint.Common;

/// <summary>
/// Cross-cutting in-app notification port (Production Readiness pass) — same pattern as
/// IAuditSink/IContentReportSink. Community and Mentorship notify a user through this interface
/// only; they never reference the Notifications module's Application layer or DbContext directly.
/// LinkUrl is a relative frontend path (e.g. "/app/community/posts/{id}") the notification's "view"
/// action navigates to — kept as a plain string rather than a typed union across every possible
/// target, since the sender always knows the concrete path and the reader never needs to parse it.
/// </summary>
public sealed record NotificationToSend(
    Guid RecipientUserId,
    string Category,
    string Title,
    string Body,
    string? LinkUrl,
    DateTimeOffset OccurredAt);

public interface INotificationSink
{
    Task SendAsync(NotificationToSend notification, CancellationToken cancellationToken);
}

/// <summary>Stable category values every sender must use — keeps values consistent across modules
/// without a shared enum forcing every module to reference a Notifications-owned type.</summary>
public static class NotificationCategories
{
    public const string CommunityActivity = "CommunityActivity";
    public const string MentorshipActivity = "MentorshipActivity";
    public const string Moderation = "Moderation";
}
