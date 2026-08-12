using MediatR;
using Waypoint.Common;

namespace Waypoint.Identity.Application.Session;

public sealed record SessionDto(Guid UserId, string Email, bool OnboardingCompleted, bool IsAdmin, bool IsModerator);

public sealed record GetSessionQuery : IRequest<SessionDto?>;

public sealed class GetSessionQueryHandler(
    ICurrentUserAccessor currentUser, IIdentityService identityService, IOnboardingStatusProvider onboardingStatus)
    : IRequestHandler<GetSessionQuery, SessionDto?>
{
    public async Task<SessionDto?> Handle(GetSessionQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return null;
        }

        var user = await identityService.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var onboardingCompleted = await onboardingStatus.HasCompletedOnboardingAsync(userId, cancellationToken);
        return new SessionDto(
            user.Id, user.Email, onboardingCompleted, currentUser.IsInRole(Roles.Admin), currentUser.IsInRole(Roles.Moderator));
    }
}
