using MediatR;
using Waypoint.Common;

namespace Waypoint.Mentorship.Application.GetMyMentorProfile;

public sealed record GetMyMentorProfileQuery : IRequest<MentorProfileDto?>;

public sealed class GetMyMentorProfileQueryHandler(
    IMentorshipRepository repository, IProfileSummaryProvider profileSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetMyMentorProfileQuery, MentorProfileDto?>
{
    public async Task<MentorProfileDto?> Handle(GetMyMentorProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var profile = await repository.GetMentorProfileByUserIdAsync(userId, cancellationToken);
        if (profile is null)
        {
            return null;
        }

        var mentor = await PersonResolver.ResolveAsync(profileSummaryProvider, userId, cancellationToken);
        return MentorProfileDto.From(profile, mentor);
    }
}
