using MediatR;
using Waypoint.Common;
using Waypoint.Mentorship.Domain;

namespace Waypoint.Mentorship.Application.GetHelpRequests;

public sealed record GetHelpRequestsQuery(HelpRequestCategory? CategoryFilter, HelpRequestStatus? StatusFilter, int Take = 100)
    : IRequest<IReadOnlyList<HelpRequestDto>>;

public sealed class GetHelpRequestsQueryHandler(
    IMentorshipRepository repository,
    IProfileSummaryProvider profileSummaryProvider,
    IDreamSummaryProvider dreamSummaryProvider,
    ICurrentUserAccessor currentUser)
    : IRequestHandler<GetHelpRequestsQuery, IReadOnlyList<HelpRequestDto>>
{
    public async Task<IReadOnlyList<HelpRequestDto>> Handle(GetHelpRequestsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");

        var take = Math.Clamp(request.Take, 1, 200);
        var requests = await repository.GetHelpRequestsAsync(request.CategoryFilter, request.StatusFilter, take, cancellationToken);
        var counts = await repository.GetResponseCountsAsync(requests.Select(r => r.Id).ToList(), cancellationToken);
        var authors = await PersonResolver.ResolveManyAsync(
            profileSummaryProvider, requests.Select(r => r.UserId).ToList(), cancellationToken);
        var attachedDreams = await AttachedDreamResolver.ResolveManyAsync(
            dreamSummaryProvider,
            requests.Where(r => r.DreamId.HasValue).Select(r => r.DreamId!.Value).ToList(),
            cancellationToken);

        // Already ordered by the repository (most-recent-first, capped at `take`).
        return requests
            .Select(r => HelpRequestDto.From(
                r, authors[r.UserId], r.DreamId.HasValue ? attachedDreams.GetValueOrDefault(r.DreamId.Value) : null,
                counts.GetValueOrDefault(r.Id), userId))
            .ToList();
    }
}
