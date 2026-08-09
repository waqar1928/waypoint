using MediatR;
using Waypoint.Common;

namespace Waypoint.Mentorship.Application.GetMentorDirectory;

public sealed record GetMentorDirectoryQuery(string? ExpertiseFilter) : IRequest<IReadOnlyList<MentorProfileDto>>;

public sealed class GetMentorDirectoryQueryHandler(
    IMentorshipRepository repository, IProfileSummaryProvider profileSummaryProvider)
    : IRequestHandler<GetMentorDirectoryQuery, IReadOnlyList<MentorProfileDto>>
{
    public async Task<IReadOnlyList<MentorProfileDto>> Handle(GetMentorDirectoryQuery request, CancellationToken cancellationToken)
    {
        var profiles = await repository.GetMentorDirectoryAsync(request.ExpertiseFilter, cancellationToken);
        var mentors = await PersonResolver.ResolveManyAsync(
            profileSummaryProvider, profiles.Select(p => p.UserId).ToList(), cancellationToken);

        return profiles
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => MentorProfileDto.From(p, mentors[p.UserId]))
            .ToList();
    }
}
