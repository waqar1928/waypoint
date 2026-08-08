using MediatR;
using Waypoint.Common;

namespace Waypoint.Users.Application.Profiles;

public sealed record GetMyProfileQuery : IRequest<ProfileDto>;

public sealed class GetMyProfileQueryHandler(IUsersRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetMyProfileQuery, ProfileDto>
{
    public async Task<ProfileDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var profile = await repository.GetProfileAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Profile not found.");

        return new ProfileDto(
            profile.UserId,
            profile.DisplayName,
            profile.Bio,
            profile.AvatarUrl,
            profile.TimeZone,
            profile.Locale,
            profile.OnboardingCompletedAt);
    }
}
