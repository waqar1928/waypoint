using FluentValidation;
using MediatR;
using Waypoint.Common;

namespace Waypoint.Identity.Application.DeleteAccount;

public sealed record DeleteAccountCommand(string Password) : IRequest;

public sealed class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountCommandValidator()
    {
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class DeleteAccountCommandHandler(
    IIdentityService identityService,
    ICurrentUserAccessor currentUser,
    IPublisher publisher,
    IAuditSink auditSink,
    IDreamSummaryProvider dreamSummaryProvider)
    : IRequestHandler<DeleteAccountCommand>
{
    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");

        var passwordValid = await identityService.CheckPasswordAsync(userId, request.Password, cancellationToken);
        if (!passwordValid)
        {
            throw new AuthenticationFailedException("Incorrect password.");
        }

        // Resolved and snapshotted *before* the account is actually deleted — see
        // UserDeletedIntegrationEvent's own doc comment for exactly why every Dream-keyed module's
        // cascade-delete handler needs this passed on the event rather than re-resolving it live.
        var dreamSummary = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken);

        await identityService.SignOutAsync(cancellationToken);

        var result = await identityService.DeleteUserAsync(userId, cancellationToken);
        if (!result.Succeeded)
        {
            throw new ConflictException("We couldn't delete your account. Please try again.");
        }

        // A real, irreversible action that had no audit trail at all — see
        // docs/PRODUCTION_READINESS_AUDIT.md's Authentication/Audit Logging sections. The Audit
        // module has no foreign key back to Identity's user table (separate schema, no
        // cross-module relational integrity per this codebase's module boundary rules), so this
        // entry correctly survives the user record it references being gone.
        await auditSink.RecordAsync(
            new AuditEntry("User", userId, "AccountDeleted", userId, null, DateTimeOffset.UtcNow),
            cancellationToken);

        await publisher.Publish(new UserDeletedIntegrationEvent(userId, dreamSummary?.DreamId), cancellationToken);
    }
}
