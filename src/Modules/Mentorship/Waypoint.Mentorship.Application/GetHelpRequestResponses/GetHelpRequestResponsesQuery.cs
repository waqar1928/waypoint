using MediatR;
using Waypoint.Common;

namespace Waypoint.Mentorship.Application.GetHelpRequestResponses;

public sealed record GetHelpRequestResponsesQuery(Guid HelpRequestId) : IRequest<IReadOnlyList<HelpRequestResponseDto>>;

public sealed class GetHelpRequestResponsesQueryHandler(
    IMentorshipRepository repository, IProfileSummaryProvider profileSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetHelpRequestResponsesQuery, IReadOnlyList<HelpRequestResponseDto>>
{
    public async Task<IReadOnlyList<HelpRequestResponseDto>> Handle(
        GetHelpRequestResponsesQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        _ = await repository.GetHelpRequestByIdAsync(request.HelpRequestId, cancellationToken)
            ?? throw new NotFoundException("Help request not found.");

        var responses = await repository.GetResponsesForHelpRequestAsync(request.HelpRequestId, cancellationToken);
        var responders = await PersonResolver.ResolveManyAsync(
            profileSummaryProvider, responses.Select(r => r.ResponderUserId).ToList(), cancellationToken);

        return responses
            .OrderBy(r => r.CreatedAt)
            .Select(r => HelpRequestResponseDto.From(r, responders[r.ResponderUserId], userId))
            .ToList();
    }
}
