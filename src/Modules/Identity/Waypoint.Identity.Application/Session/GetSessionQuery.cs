using MediatR;
using Waypoint.Common;

namespace Waypoint.Identity.Application.Session;

public sealed record SessionDto(Guid UserId, string Email, bool OnboardingCompleted);

public sealed record GetSessionQuery : IRequest<SessionDto?>;

public sealed class GetSessionQueryHandler(ICurrentUserAccessor currentUser, IIdentityService identityService)
    : IRequestHandler<GetSessionQuery, SessionDto?>
{
    public async Task<SessionDto?> Handle(GetSessionQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return null;
        }

        var user = await identityService.FindByIdAsync(userId, cancellationToken);
        return user is null ? null : new SessionDto(user.Id, user.Email, OnboardingCompleted: false);
    }
}
