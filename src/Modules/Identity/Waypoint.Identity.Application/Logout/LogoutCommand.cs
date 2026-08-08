using MediatR;

namespace Waypoint.Identity.Application.Logout;

public sealed record LogoutCommand : IRequest;

public sealed class LogoutCommandHandler(IIdentityService identityService) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await identityService.SignOutAsync(cancellationToken);
    }
}
