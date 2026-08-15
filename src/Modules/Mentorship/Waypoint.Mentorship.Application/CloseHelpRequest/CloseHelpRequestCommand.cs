using MediatR;
using Waypoint.Common;
using Waypoint.Mentorship.Domain;

namespace Waypoint.Mentorship.Application.CloseHelpRequest;

public sealed record CloseHelpRequestCommand(Guid HelpRequestId) : IRequest<HelpRequestDto>;

public sealed class CloseHelpRequestCommandHandler(
    IMentorshipRepository repository,
    IProfileSummaryProvider profileSummaryProvider,
    IDreamSummaryProvider dreamSummaryProvider,
    ICurrentUserAccessor currentUser)
    : IRequestHandler<CloseHelpRequestCommand, HelpRequestDto>
{
    public async Task<HelpRequestDto> Handle(CloseHelpRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var helpRequest = await repository.GetHelpRequestByIdAsync(request.HelpRequestId, cancellationToken)
            ?? throw new NotFoundException("Help request not found.");

        if (helpRequest.UserId != userId)
        {
            throw new NotFoundException("Help request not found.");
        }

        helpRequest.Status = HelpRequestStatus.Closed;
        helpRequest.UpdatedBy = userId;
        await repository.SaveHelpRequestAsync(helpRequest, cancellationToken);

        var counts = await repository.GetResponseCountsAsync([helpRequest.Id], cancellationToken);
        var author = await PersonResolver.ResolveAsync(profileSummaryProvider, userId, cancellationToken);
        var attachedDream = await AttachedDreamResolver.ResolveAsync(dreamSummaryProvider, helpRequest.DreamId, cancellationToken);
        return HelpRequestDto.From(helpRequest, author, attachedDream, counts.GetValueOrDefault(helpRequest.Id), userId);
    }
}
