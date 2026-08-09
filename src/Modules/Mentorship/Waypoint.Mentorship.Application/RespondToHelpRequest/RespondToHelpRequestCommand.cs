using FluentValidation;
using MediatR;
using Waypoint.Common;
using Waypoint.Mentorship.Domain;

namespace Waypoint.Mentorship.Application.RespondToHelpRequest;

public sealed record RespondToHelpRequestCommand(Guid HelpRequestId, string Body) : IRequest<HelpRequestResponseDto>;

public sealed class RespondToHelpRequestCommandValidator : AbstractValidator<RespondToHelpRequestCommand>
{
    public RespondToHelpRequestCommandValidator()
    {
        RuleFor(x => x.HelpRequestId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty().MaximumLength(2000);
    }
}

public sealed class RespondToHelpRequestCommandHandler(
    IMentorshipRepository repository, IProfileSummaryProvider profileSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<RespondToHelpRequestCommand, HelpRequestResponseDto>
{
    public async Task<HelpRequestResponseDto> Handle(RespondToHelpRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var helpRequest = await repository.GetHelpRequestByIdAsync(request.HelpRequestId, cancellationToken)
            ?? throw new NotFoundException("Help request not found.");

        if (helpRequest.Status == HelpRequestStatus.Closed)
        {
            throw new ConflictException("This help request is closed.");
        }

        var response = HelpRequestResponse.Create(helpRequest.Id, userId, request.Body);
        await repository.AddResponseAsync(response, cancellationToken);

        if (helpRequest.Status == HelpRequestStatus.Open)
        {
            helpRequest.Status = HelpRequestStatus.Answered;
            helpRequest.UpdatedBy = userId;
            await repository.SaveHelpRequestAsync(helpRequest, cancellationToken);
        }

        var responder = await PersonResolver.ResolveAsync(profileSummaryProvider, userId, cancellationToken);
        return HelpRequestResponseDto.From(response, responder, userId);
    }
}
