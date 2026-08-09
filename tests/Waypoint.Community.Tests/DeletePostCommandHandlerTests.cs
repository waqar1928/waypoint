using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Community.Application;
using Waypoint.Community.Application.DeletePost;
using Waypoint.Community.Domain;
using Xunit;

namespace Waypoint.Community.Tests;

public class DeletePostCommandHandlerTests
{
    private readonly ICommunityRepository _repository = Substitute.For<ICommunityRepository>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _userId = Guid.NewGuid();

    private DeletePostCommandHandler CreateHandler() => new(_repository, _currentUser);

    [Fact]
    public async Task Throws_when_deleting_someone_elses_post()
    {
        _currentUser.UserId.Returns(_userId);
        var othersPost = CommunityPost.Create(Guid.NewGuid(), null, "Not yours", PostVisibility.Community);
        _repository.GetPostByIdAsync(othersPost.Id, Arg.Any<CancellationToken>()).Returns(othersPost);

        var act = () => CreateHandler().Handle(new DeletePostCommand(othersPost.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _repository.DidNotReceive().SavePostAsync(Arg.Any<CommunityPost>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Soft_deletes_own_post()
    {
        _currentUser.UserId.Returns(_userId);
        var post = CommunityPost.Create(_userId, null, "Mine", PostVisibility.Community);
        _repository.GetPostByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);

        await CreateHandler().Handle(new DeletePostCommand(post.Id), CancellationToken.None);

        post.DeletedAt.Should().NotBeNull();
        await _repository.Received(1).SavePostAsync(post, Arg.Any<CancellationToken>());
    }
}
