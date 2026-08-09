using FluentValidation;
using MediatR;
using Waypoint.Common;
using Waypoint.Mentorship.Domain;

namespace Waypoint.Mentorship.Application.CreateHelpRequest;

public sealed record CreateHelpRequestCommand(
    HelpRequestCategory Category, string Title, string Body, Guid? DreamId) : IRequest<HelpRequestDto>;

public sealed class CreateHelpRequestCommandValidator : AbstractValidator<CreateHelpRequestCommand>
{
    public CreateHelpRequestCommandValidator()
    {
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(2000);
    }
}

public sealed class CreateHelpRequestCommandHandler(
    IMentorshipRepository repository, IProfileSummaryProvider profileSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<CreateHelpRequestCommand, HelpRequestDto>
{
    public async Task<HelpRequestDto> Handle(CreateHelpRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");

        var helpRequest = HelpRequest.Create(userId, request.DreamId, request.Category, request.Title, request.Body);
        await repository.AddHelpRequestAsync(helpRequest, cancellationToken);

        var author = await PersonResolver.ResolveAsync(profileSummaryProvider, userId, cancellationToken);
        return HelpRequestDto.From(helpRequest, author, responseCount: 0, userId);
    }
}
