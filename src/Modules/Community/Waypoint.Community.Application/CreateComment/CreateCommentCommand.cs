using FluentValidation;
using MediatR;
using Waypoint.Common;
using Waypoint.Community.Domain;

namespace Waypoint.Community.Application.CreateComment;

public sealed record CreateCommentCommand(Guid PostId, string Body) : IRequest<CommentDto>;

public sealed class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.PostId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty().MaximumLength(1000);
    }
}

public sealed class CreateCommentCommandHandler(
    ICommunityRepository repository, IProfileSummaryProvider profileSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<CreateCommentCommand, CommentDto>
{
    public async Task<CommentDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var post = await repository.GetPostByIdAsync(request.PostId, cancellationToken)
            ?? throw new NotFoundException("Post not found.");

        // Only the owner can comment on their own Private post; Community/Public posts are open.
        if (post.Visibility == PostVisibility.Private && post.UserId != userId)
        {
            throw new NotFoundException("Post not found.");
        }

        var comment = Comment.Create(request.PostId, userId, request.Body);
        await repository.AddCommentAsync(comment, cancellationToken);

        var author = await AuthorResolver.ResolveAsync(profileSummaryProvider, userId, cancellationToken);
        return CommentDto.From(comment, author, userId);
    }
}
