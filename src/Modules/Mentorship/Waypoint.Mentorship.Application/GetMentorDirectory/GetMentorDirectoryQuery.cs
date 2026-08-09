using MediatR;
using Waypoint.Common;

namespace Waypoint.Mentorship.Application.GetMentorDirectory;

// Take defaults to the public-directory-sized cap; the admin oversight endpoint (which must see
// every mentor, not just the most recent page) explicitly passes a much higher value rather than
// relying on this default — see MentorshipEndpoints.cs's `/api/v1/admin/mentors` mapping.
public sealed record GetMentorDirectoryQuery(string? ExpertiseFilter, int Take = 100) : IRequest<IReadOnlyList<MentorProfileDto>>;

public sealed class GetMentorDirectoryQueryHandler(
    IMentorshipRepository repository, IProfileSummaryProvider profileSummaryProvider)
    : IRequestHandler<GetMentorDirectoryQuery, IReadOnlyList<MentorProfileDto>>
{
    public async Task<IReadOnlyList<MentorProfileDto>> Handle(GetMentorDirectoryQuery request, CancellationToken cancellationToken)
    {
        var take = Math.Clamp(request.Take, 1, 5000);
        var profiles = await repository.GetMentorDirectoryAsync(request.ExpertiseFilter, take, cancellationToken);
        var mentors = await PersonResolver.ResolveManyAsync(
            profileSummaryProvider, profiles.Select(p => p.UserId).ToList(), cancellationToken);

        // Already ordered by the repository (most-recent-first, capped at `take`).
        return profiles
            .Select(p => MentorProfileDto.From(p, mentors[p.UserId]))
            .ToList();
    }
}
