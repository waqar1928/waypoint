namespace Waypoint.Notifications.Application.Push;

public sealed record PushPayload(string Title, string Body, string Url);

/// <summary>
/// The ONE place that decides what a push notification actually says. Enforced server-side, at
/// payload-construction time, inside ScheduledNotificationWorker - never left to the service
/// worker, which only ever displays whatever payload it's handed (see apps/web/public/sw.js's push
/// handler, which contains no content-decision logic of its own). Default content reveals nothing
/// about what the user's plan actually contains - safe to appear on a lock screen in a shared or
/// public setting. Detailed content is only ever built when NotificationPreferences.
/// PushDetailedContent is explicitly true for that user, and even then falls back to the same
/// generic copy if there's no actual next move to name (defensive - should be unreachable in
/// practice, since the worker already skips sending entirely when there's no next best action).
/// </summary>
public static class PushPayloadBuilder
{
    public const string DefaultTitle = "Drevia";
    public const string DefaultBody = "Your next move is ready.";
    public const string DefaultUrl = "/app/actions";

    public static PushPayload BuildDailyNextMove(bool detailedContentEnabled, string? nextMoveTitle)
    {
        if (!detailedContentEnabled || string.IsNullOrWhiteSpace(nextMoveTitle))
        {
            return new PushPayload(DefaultTitle, DefaultBody, DefaultUrl);
        }

        return new PushPayload(DefaultTitle, nextMoveTitle, DefaultUrl);
    }
}
