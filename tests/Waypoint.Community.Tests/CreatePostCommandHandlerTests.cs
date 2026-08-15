using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Community.Application;
using Waypoint.Community.Application.CreatePost;
using Waypoint.Community.Domain;
using Xunit;

namespace Waypoint.Community.Tests;

public class CreatePostCommandHandlerTests
{
    private readonly ICommunityRepository _repository = Substitute.For<ICommunityRepository>();
    private readonly IProfileSummaryProvider _profileSummaryProvider = Substitute.For<IProfileSummaryProvider>();
    private readonly IDreamSummaryProvider _dreamSummaryProvider = Substitute.For<IDreamSummaryProvider>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _userId = Guid.NewGuid();

    private CreatePostCommandHandler CreateHandler() =>
        new(_repository, _profileSummaryProvider, _dreamSummaryProvider, _currentUser);

    [Fact]
    public async Task Does_not_attach_a_dream_when_AttachDream_is_false()
    {
        _currentUser.UserId.Returns(_userId);
        _profileSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new ProfileSummary(_userId, "Sam Rivera", null));

        var result = await CreateHandler().Handle(
            new CreatePostCommand("Body", PostVisibility.Community, AttachDream: false), CancellationToken.None);

        result.AttachedDream.Should().BeNull();
        // AttachDream defaults to off, so a normal post never even queries for a Dream - not just
        // that the result excludes one.
        await _dreamSummaryProvider.DidNotReceive().GetForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).AddPostAsync(
            Arg.Is<CommunityPost>(p => p.DreamId == null), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Attaches_the_current_users_own_dream_when_opted_in()
    {
        var dreamId = Guid.NewGuid();
        _currentUser.UserId.Returns(_userId);
        _profileSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new ProfileSummary(_userId, "Sam Rivera", null));
        _dreamSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns(
            new DreamSummary(dreamId, _userId, "Cut waste for shops", "Statement text", null, null, null, null, null, null, false));

        var result = await CreateHandler().Handle(
            new CreatePostCommand("Body", PostVisibility.Community, AttachDream: true), CancellationToken.None);

        result.AttachedDream.Should().NotBeNull();
        result.AttachedDream!.Title.Should().Be("Cut waste for shops");
        result.AttachedDream.Statement.Should().Be("Statement text");
        await _repository.Received(1).AddPostAsync(
            Arg.Is<CommunityPost>(p => p.DreamId == dreamId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Never_accepts_a_dream_id_from_the_caller_only_resolves_the_signed_in_users_own_dream()
    {
        // Regression test for the actual security fix: CreatePostCommand used to take a raw
        // Guid? DreamId straight from the client, with no ownership check anywhere - a malicious
        // request could attach (and thereby publicly expose the title/statement of) any other
        // user's Dream. The fix removes that parameter entirely; there is no way to express
        // "attach someone else's Dream" in this command's shape at all, since AttachDream is a
        // bool and the handler only ever calls dreamSummaryProvider.GetForUserAsync(userId, ...)
        // — the *signed-in* user's id, never anything from the request body. This test exists so
        // that if AttachDream is ever changed back to accept an id, it fails loudly.
        var attackerId = Guid.NewGuid();
        var victimDreamId = Guid.NewGuid();
        _currentUser.UserId.Returns(attackerId);
        _profileSummaryProvider.GetForUserAsync(attackerId, Arg.Any<CancellationToken>())
            .Returns(new ProfileSummary(attackerId, "Attacker", null));
        // The attacker has no Dream of their own.
        _dreamSummaryProvider.GetForUserAsync(attackerId, Arg.Any<CancellationToken>()).Returns((DreamSummary?)null);

        var result = await CreateHandler().Handle(
            new CreatePostCommand("Body", PostVisibility.Community, AttachDream: true), CancellationToken.None);

        result.AttachedDream.Should().BeNull();
        await _dreamSummaryProvider.DidNotReceive().GetByIdAsync(victimDreamId, Arg.Any<CancellationToken>());
        await _repository.Received(1).AddPostAsync(
            Arg.Is<CommunityPost>(p => p.DreamId == null), Arg.Any<CancellationToken>());
    }
}
