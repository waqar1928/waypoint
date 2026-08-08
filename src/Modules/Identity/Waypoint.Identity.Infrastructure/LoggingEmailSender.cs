using Microsoft.Extensions.Logging;
using Waypoint.Identity.Application;

namespace Waypoint.Identity.Infrastructure;

/// <summary>
/// Local/dev email delivery: logs the message instead of sending it, since
/// no transactional email provider is configured yet. Swap in a real
/// adapter (SendGrid, SES, Postmark, ...) behind this same IEmailSender port
/// when one is provisioned — no Application-layer code changes required.
/// </summary>
public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email (dev mode, not delivered) — To: {ToEmail}, Subject: {Subject}\n{Body}",
            toEmail, subject, htmlBody);
        return Task.CompletedTask;
    }
}
