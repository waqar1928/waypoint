using FluentValidation;
using MediatR;
using Waypoint.Common;
using Waypoint.Community.Domain;

namespace Waypoint.Community.Application.CreatePost;

public sealed record CreatePostCommand(string Body, PostVisibility Visibility, Guid? DreamId) : IRequest<PostDto>;

public sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Visibility).IsInEnum();
    }
}

public sealed class CreatePostCommandHandler(
    ICommunityRepository repository, IProfileSummaryProvider profileSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<CreatePostCommand, PostDto>
{
    public async Task<PostDto> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");

        var post = CommunityPost.Create(userId, request.DreamId, request.Body, request.Visibility);
        await repository.AddPostAsync(post, cancellationToken);

        var author = await AuthorResolver.ResolveAsync(profileSummaryProvider, userId, cancellationToken);
        return PostDto.From(post, author, commentCount: 0, userId);
    }
}
