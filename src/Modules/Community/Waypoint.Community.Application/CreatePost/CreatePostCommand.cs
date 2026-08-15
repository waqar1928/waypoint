using FluentValidation;
using MediatR;
using Waypoint.Common;
using Waypoint.Community.Domain;

namespace Waypoint.Community.Application.CreatePost;

/// <summary>
/// AttachDream is a boolean, never a client-supplied DreamId - the handler resolves the
/// authenticated user's own Dream server-side (see IDreamSummaryProvider.GetForUserAsync). This
/// is deliberate: accepting a raw DreamId from the client would let a malicious request attach
/// (and thereby publicly expose) any user's Dream, not just their own.
/// </summary>
public sealed record CreatePostCommand(string Body, PostVisibility Visibility, bool AttachDream) : IRequest<PostDto>;

public sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Visibility).IsInEnum();
    }
}

public sealed class CreatePostCommandHandler(
    ICommunityRepository repository,
    IProfileSummaryProvider profileSummaryProvider,
    IDreamSummaryProvider dreamSummaryProvider,
    ICurrentUserAccessor currentUser)
    : IRequestHandler<CreatePostCommand, PostDto>
{
    public async Task<PostDto> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");

        // Resolving the user's own Dream here (rather than trusting a client-supplied Id) is what
        // makes this safe — see the doc comment on CreatePostCommand.
        DreamSummary? dream = null;
        if (request.AttachDream)
        {
            dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken);
        }

        var post = CommunityPost.Create(userId, dream?.DreamId, request.Body, request.Visibility);
        await repository.AddPostAsync(post, cancellationToken);

        var author = await AuthorResolver.ResolveAsync(profileSummaryProvider, userId, cancellationToken);
        var attachedDream = dream is null ? null : new AttachedDreamDto(dream.Title, dream.Statement);
        return PostDto.From(post, author, attachedDream, commentCount: 0, userId);
    }
}
