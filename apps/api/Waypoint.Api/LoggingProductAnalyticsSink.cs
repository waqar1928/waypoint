using Waypoint.Common;

namespace Waypoint.Api;

/// <summary>
/// Real product-analytics delivery today: a structured log line, not a persisted table or a
/// vendor API call. See docs/PRODUCTION_READINESS_AUDIT.md's Analytics section — deliberately
/// not building a new database-backed module for this (that would just be a self-hosted
/// analytics vendor few would choose over a real one), and deliberately not wiring a specific
/// paid analytics vendor without the user's explicit choice (that's on this project's own
/// stop-list: "purchasing services"). This still has real value on its own: every event already
/// flows through the same structured JSON logging pipeline set up in Phase 12
/// (docs/13-production-readiness-phase12.md), which a real log aggregator can already index and
/// query today. Swapping to a real analytics vendor later is a one-class, one-DI-registration
/// change — same swap-ready pattern as IAiService/IEmailSender.
/// </summary>
public sealed class LoggingProductAnalyticsSink(ILogger<LoggingProductAnalyticsSink> logger) : IProductAnalyticsSink
{
    public Task TrackAsync(AnalyticsEvent analyticsEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Analytics event {EventName} for user {UserId} at {OccurredAt} — properties: {@Properties}",
            analyticsEvent.Name, analyticsEvent.UserId, analyticsEvent.OccurredAt, analyticsEvent.PropertiesRedacted);
        return Task.CompletedTask;
    }
}
