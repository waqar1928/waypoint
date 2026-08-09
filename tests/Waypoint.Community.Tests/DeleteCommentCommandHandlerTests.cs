using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Community.Application;
using Waypoint.Community.Application.DeleteComment;
using Waypoint.Community.Domain;
using Xunit;

namespace Waypoint.Community.Tests;

public class DeleteCommentCommandHandlerTests
{
    private readonly ICommunityRepository _repository = Substitute.For<ICommunityRepository>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _userId = Guid.NewGuid();

    private DeleteCommentCommandHandler CreateHandler() => new(_repository, _currentUser);

    [Fact]
    public async Task Soft_deletes_own_comment()
    {
        _currentUser.UserId.Returns(_userId);
        var comment = Comment.Create(Guid.NewGuid(), _userId, "Mine");
        _repository.GetCommentByIdAsync(comment.Id, Arg.Any<CancellationToken>()).Returns(comment);

        await CreateHandler().Handle(new DeleteCommentCommand(comment.Id), CancellationToken.None);

        comment.DeletedAt.Should().NotBeNull();
        await _repository.Received(1).SaveCommentAsync(comment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_not_found_rather_than_forbidden_when_deleting_someone_elses_comment()
    {
        // Deliberately masks the ownership failure as NotFound (not Forbidden) so a non-owner can't
        // use this endpoint to probe whether a given comment ID exists at all.
        _currentUser.UserId.Returns(_userId);
        var othersComment = Comment.Create(Guid.NewGuid(), Guid.NewGuid(), "Not yours");
        _repository.GetCommentByIdAsync(othersComment.Id, Arg.Any<CancellationToken>()).Returns(othersComment);

        var act = () => CreateHandler().Handle(new DeleteCommentCommand(othersComment.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _repository.DidNotReceive().SaveCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_when_the_comment_does_not_exist()
    {
        _currentUser.UserId.Returns(_userId);
        _repository.GetCommentByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Comment?)null);

        var act = () => CreateHandler().Handle(new DeleteCommentCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
