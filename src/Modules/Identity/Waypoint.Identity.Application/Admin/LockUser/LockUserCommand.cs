using MediatR;
using Waypoint.Common;

namespace Waypoint.Identity.Application.Admin.LockUser;

public sealed record LockUserCommand(Guid UserId) : IRequest;

public sealed class LockUserCommandHandler(IIdentityService identityService, IAuditSink auditSink, ICurrentUserAccessor currentUser)
    : IRequestHandler<LockUserCommand>
{
    public async Task Handle(LockUserCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.LockUserAsync(request.UserId, cancellationToken);
        if (!result.Succeeded)
        {
            throw new NotFoundException("User not found.");
        }

        await auditSink.RecordAsync(
            new AuditEntry("User", request.UserId, "LockedByAdmin", currentUser.UserId, null, DateTimeOffset.UtcNow),
            cancellationToken);
    }
}
