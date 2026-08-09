using MediatR;
using Waypoint.Common;

namespace Waypoint.Identity.Application.Admin.UnlockUser;

public sealed record UnlockUserCommand(Guid UserId) : IRequest;

public sealed class UnlockUserCommandHandler(IIdentityService identityService, IAuditSink auditSink, ICurrentUserAccessor currentUser)
    : IRequestHandler<UnlockUserCommand>
{
    public async Task Handle(UnlockUserCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.UnlockUserAsync(request.UserId, cancellationToken);
        if (!result.Succeeded)
        {
            throw new NotFoundException("User not found.");
        }

        await auditSink.RecordAsync(
            new AuditEntry("User", request.UserId, "UnlockedByAdmin", currentUser.UserId, null, DateTimeOffset.UtcNow),
            cancellationToken);
    }
}
