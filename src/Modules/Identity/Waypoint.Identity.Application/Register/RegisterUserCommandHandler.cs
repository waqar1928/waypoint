using MediatR;
using Microsoft.Extensions.Options;
using Waypoint.Common;

namespace Waypoint.Identity.Application.Register;

public sealed class RegisterUserCommandHandler(
    IIdentityService identityService,
    IEmailSender emailSender,
    IPublisher publisher,
    IAuditSink auditSink,
    IProductAnalyticsSink analyticsSink,
    IOptions<IdentityLinkOptions> linkOptions)
    : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var (result, userId) = await identityService.CreateUserAsync(request.Email, request.Password, cancellationToken);

        if (!result.Succeeded || userId is null)
        {
            throw new ConflictException("An account with this email already exists.");
        }

        await auditSink.RecordAsync(
            new AuditEntry("User", userId.Value, "Registered", userId.Value, null, DateTimeOffset.UtcNow),
            cancellationToken);
        await analyticsSink.TrackAsync(
            new AnalyticsEvent(AnalyticsEvents.UserRegistered, userId.Value, null, DateTimeOffset.UtcNow),
            cancellationToken);

        var token = await identityService.GenerateEmailConfirmationTokenAsync(userId.Value, cancellationToken);
        var verificationLink =
            $"{linkOptions.Value.WebAppBaseUrl}/verify-email?userId={userId}&token={Uri.EscapeDataString(token)}";

        await emailSender.SendAsync(
            request.Email,
            "Confirm your Waypoint account",
            $"""<p>Welcome to Waypoint. Confirm your email to get started:</p><p><a href="{verificationLink}">Confirm email</a></p>""",
            cancellationToken);

        await publisher.Publish(
            new UserRegisteredIntegrationEvent(userId.Value, request.DisplayName, request.Email),
            cancellationToken);

        return new RegisterUserResult(userId.Value, request.Email, EmailConfirmationSent: true);
    }
}
