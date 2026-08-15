using FluentValidation;
using MediatR;
using Waypoint.Common;
using Waypoint.Mentorship.Domain;

namespace Waypoint.Mentorship.Application.CreateHelpRequest;

/// <summary>
/// AttachDream is a boolean, never a client-supplied DreamId - same reasoning as Community's
/// CreatePostCommand. The handler resolves the authenticated user's own Dream server-side rather
/// than trusting anything the client sends, so there's no way to attach (and thereby expose to a
/// mentor) a Dream that isn't yours.
/// </summary>
public sealed record CreateHelpRequestCommand(
    HelpRequestCategory Category, string Title, string Body, bool AttachDream) : IRequest<HelpRequestDto>;

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
    IMentorshipRepository repository,
    IProfileSummaryProvider profileSummaryProvider,
    IDreamSummaryProvider dreamSummaryProvider,
    ICurrentUserAccessor currentUser)
    : IRequestHandler<CreateHelpRequestCommand, HelpRequestDto>
{
    public async Task<HelpRequestDto> Handle(CreateHelpRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");

        DreamSummary? dream = null;
        if (request.AttachDream)
        {
            dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken);
        }

        var helpRequest = HelpRequest.Create(userId, dream?.DreamId, request.Category, request.Title, request.Body);
        await repository.AddHelpRequestAsync(helpRequest, cancellationToken);

        var author = await PersonResolver.ResolveAsync(profileSummaryProvider, userId, cancellationToken);
        var attachedDream = dream is null ? null : new AttachedDreamDto(dream.Title, dream.Statement);
        return HelpRequestDto.From(helpRequest, author, attachedDream, responseCount: 0, userId);
    }
}
