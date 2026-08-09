using MediatR;
using Waypoint.Common;
using Waypoint.Mentorship.Domain;

namespace Waypoint.Mentorship.Application.GetHelpRequests;

public sealed record GetHelpRequestsQuery(HelpRequestCategory? CategoryFilter, HelpRequestStatus? StatusFilter)
    : IRequest<IReadOnlyList<HelpRequestDto>>;

public sealed class GetHelpRequestsQueryHandler(
    IMentorshipRepository repository, IProfileSummaryProvider profileSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetHelpRequestsQuery, IReadOnlyList<HelpRequestDto>>
{
    public async Task<IReadOnlyList<HelpRequestDto>> Handle(GetHelpRequestsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");

        var requests = await repository.GetHelpRequestsAsync(request.CategoryFilter, request.StatusFilter, cancellationToken);
        var counts = await repository.GetResponseCountsAsync(requests.Select(r => r.Id).ToList(), cancellationToken);
        var authors = await PersonResolver.ResolveManyAsync(
            profileSummaryProvider, requests.Select(r => r.UserId).ToList(), cancellationToken);

        return requests
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => HelpRequestDto.From(r, authors[r.UserId], counts.GetValueOrDefault(r.Id), userId))
            .ToList();
    }
}
