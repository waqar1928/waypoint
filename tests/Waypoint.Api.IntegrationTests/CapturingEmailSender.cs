using System.Collections.Concurrent;
using Waypoint.Identity.Application;

namespace Waypoint.Api.IntegrationTests;

/// <summary>
/// Test double swapped in for the real IEmailSender (see WaypointApiFactory.ConfigureWebHost) so
/// integration tests can complete the real email-verification flow without a real inbox — added
/// once RequireConfirmedAccount started actually blocking login for unconfirmed accounts (see
/// docs/PRODUCTION_READINESS_AUDIT.md's Authentication section), which broke this suite's
/// register-then-login test until it started confirming first, same as a real user would.
/// Captures every send in memory so a test can pull the real verification/reset link out of the
/// email body exactly like a real user would click it — not a hardcoded/faked token.
/// </summary>
public sealed class CapturingEmailSender : IEmailSender
{
    public ConcurrentBag<(string ToEmail, string Subject, string HtmlBody)> SentEmails { get; } = [];

    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        SentEmails.Add((toEmail, subject, htmlBody));
        return Task.CompletedTask;
    }
}
