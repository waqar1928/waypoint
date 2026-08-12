namespace Waypoint.Common;

/// <summary>
/// Cross-cutting product-analytics port (Production Readiness pass) — same pattern as
/// IAuditSink/INotificationSink. Modules emit events through this interface only, never coupling
/// business handlers to a specific analytics vendor. See
/// docs/PRODUCTION_READINESS_AUDIT.md's Analytics section: no product analytics existed before
/// this, meaning zero visibility into activation/retention/feature usage post-launch.
///
/// PropertiesRedacted mirrors AuditEntry's PayloadRedacted naming — a reminder to whoever adds a
/// new emission call that event properties flow into logs (and eventually a real analytics
/// vendor) and must never carry PII/secrets, only coarse, aggregable facts (a category, a count,
/// an enum value — never an email, a message body, or a token).
/// </summary>
public sealed record AnalyticsEvent(
    string Name,
    Guid? UserId,
    IReadOnlyDictionary<string, string>? PropertiesRedacted,
    DateTimeOffset OccurredAt);

public interface IProductAnalyticsSink
{
    Task TrackAsync(AnalyticsEvent analyticsEvent, CancellationToken cancellationToken);
}

/// <summary>Stable event-name constants every emitter must use — same reasoning as
/// NotificationCategories: keeps values consistent across modules without a shared enum forcing
/// every module to reference a common-owned type they'd otherwise need a project reference for.</summary>
public static class AnalyticsEvents
{
    public const string UserRegistered = "user.registered";
    public const string OnboardingCompleted = "onboarding.completed";
    public const string DreamCreated = "dream.created";
    public const string ActionCompleted = "action.completed";
    public const string MilestoneAchieved = "milestone.achieved";
    public const string AiConversationStarted = "ai.conversation_started";
}
