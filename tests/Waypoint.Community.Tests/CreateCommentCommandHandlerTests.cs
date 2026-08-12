using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Community.Application;
using Waypoint.Community.Application.CreateComment;
using Waypoint.Community.Domain;
using Xunit;

namespace Waypoint.Community.Tests;

public class CreateCommentCommandHandlerTests
{
    private readonly ICommunityRepository _repository = Substitute.For<ICommunityRepository>();
    private readonly IProfileSummaryProvider _profileSummaryProvider = Substitute.For<IProfileSummaryProvider>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly INotificationSink _notificationSink = Substitute.For<INotificationSink>();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _otherUserId = Guid.NewGuid();

    private CreateCommentCommandHandler CreateHandler() =>
        new(_repository, _profileSummaryProvider, _currentUser, _notificationSink);

    [Fact]
    public async Task Throws_when_commenting_on_someone_elses_private_post()
    {
        _currentUser.UserId.Returns(_userId);
        var post = CommunityPost.Create(_otherUserId, null, "Private thoughts", PostVisibility.Private);
        _repository.GetPostByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);

        var act = () => CreateHandler().Handle(new CreateCommentCommand(post.Id, "Hi"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _repository.DidNotReceive().AddCommentAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Allows_commenting_on_a_community_visible_post()
    {
        _currentUser.UserId.Returns(_userId);
        var post = CommunityPost.Create(_otherUserId, null, "Progress update", PostVisibility.Community);
        _repository.GetPostByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);
        _profileSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new ProfileSummary(_userId, "Sam Rivera", null));

        var result = await CreateHandler().Handle(new CreateCommentCommand(post.Id, "Nice progress!"), CancellationToken.None);

        result.Body.Should().Be("Nice progress!");
        result.IsMine.Should().BeTrue();
        await _repository.Received(1).AddCommentAsync(
            Arg.Is<Comment>(c => c.PostId == post.Id && c.UserId == _userId), Arg.Any<CancellationToken>());
        await _notificationSink.Received(1).SendAsync(
            Arg.Is<NotificationToSend>(n => n.RecipientUserId == _otherUserId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Does_not_notify_when_commenting_on_your_own_post()
    {
        _currentUser.UserId.Returns(_userId);
        var post = CommunityPost.Create(_userId, null, "My own update", PostVisibility.Community);
        _repository.GetPostByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);
        _profileSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new ProfileSummary(_userId, "Sam Rivera", null));

        await CreateHandler().Handle(new CreateCommentCommand(post.Id, "Following up on my own post"), CancellationToken.None);

        await _notificationSink.DidNotReceive().SendAsync(Arg.Any<NotificationToSend>(), Arg.Any<CancellationToken>());
    }
}
