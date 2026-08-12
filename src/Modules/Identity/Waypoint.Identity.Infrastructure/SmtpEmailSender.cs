using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Waypoint.Identity.Application;

namespace Waypoint.Identity.Infrastructure;

public sealed class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromAddress { get; set; } = "no-reply@example.com";
    public string FromName { get; set; } = "Waypoint";
}

/// <summary>
/// Real email delivery over generic SMTP — registered instead of
/// <see cref="LoggingEmailSender"/> only when <c>Email:Smtp:Host</c> is
/// actually configured (see DependencyInjection.cs), so a deployment that
/// hasn't set up a mail relay yet keeps the safe logging behavior rather
/// than silently failing to send.
///
/// Uses the framework's built-in System.Net.Mail.SmtpClient rather than
/// pulling in a new NuGet dependency — Microsoft's own guidance steers new
/// code toward MailKit for advanced scenarios (OAuth2, connection pooling
/// under heavy load), but for a straightforward relay-through-SMTP-server
/// send, SmtpClient is still correct and adds no new dependency surface to
/// review. Swap this class for a provider-specific SDK (SendGrid, SES,
/// Postmark) behind the same IEmailSender port if/when real delivery volume
/// or provider-specific features (bounce webhooks, templates) justify it —
/// no Application-layer code changes required either way.
///
/// Per this project's own production-readiness rules, nothing in this class
/// is wired to actually send anything until an operator supplies real SMTP
/// credentials via configuration — that's an explicit, separate decision
/// from writing this adapter.
/// </summary>
public sealed class SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var smtp = options.Value;

        using var client = new SmtpClient(smtp.Host, smtp.Port)
        {
            EnableSsl = smtp.EnableSsl,
        };

        if (!string.IsNullOrWhiteSpace(smtp.Username))
        {
            client.Credentials = new NetworkCredential(smtp.Username, smtp.Password);
        }

        using var message = new MailMessage
        {
            From = new MailAddress(smtp.FromAddress, smtp.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };
        message.To.Add(toEmail);

        try
        {
            await client.SendMailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            // A failed send must never surface the recipient's email or the message body in logs
            // (PII), and must never bubble up as an unhandled exception that could turn into a 500
            // with details in a lower environment — log the failure type/host only and swallow it,
            // matching the existing pattern where email delivery is best-effort from the caller's
            // perspective (registration/password-reset already succeed or fail independently of
            // whether the notification email actually arrives).
            logger.LogError(ex, "Failed to send email via SMTP host {SmtpHost}:{SmtpPort}", smtp.Host, smtp.Port);
        }
    }
}
