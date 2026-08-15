using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Mentorship.Application;
using Waypoint.Mentorship.Application.CreateHelpRequest;
using Waypoint.Mentorship.Domain;
using Xunit;

namespace Waypoint.Mentorship.Tests;

public class CreateHelpRequestCommandHandlerTests
{
    private readonly IMentorshipRepository _repository = Substitute.For<IMentorshipRepository>();
    private readonly IProfileSummaryProvider _profileSummaryProvider = Substitute.For<IProfileSummaryProvider>();
    private readonly IDreamSummaryProvider _dreamSummaryProvider = Substitute.For<IDreamSummaryProvider>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _userId = Guid.NewGuid();

    private CreateHelpRequestCommandHandler CreateHandler() =>
        new(_repository, _profileSummaryProvider, _dreamSummaryProvider, _currentUser);

    [Fact]
    public async Task Does_not_attach_a_dream_when_AttachDream_is_false()
    {
        _currentUser.UserId.Returns(_userId);
        _profileSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new ProfileSummary(_userId, "Sam Rivera", null));

        var result = await CreateHandler().Handle(
            new CreateHelpRequestCommand(HelpRequestCategory.Business, "Title", "Body", AttachDream: false),
            CancellationToken.None);

        result.AttachedDream.Should().BeNull();
        result.DreamId.Should().BeNull();
        await _dreamSummaryProvider.DidNotReceive().GetForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
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
            new CreateHelpRequestCommand(HelpRequestCategory.Business, "Title", "Body", AttachDream: true),
            CancellationToken.None);

        result.DreamId.Should().Be(dreamId);
        result.AttachedDream.Should().NotBeNull();
        result.AttachedDream!.Title.Should().Be("Cut waste for shops");
        await _repository.Received(1).AddHelpRequestAsync(
            Arg.Is<HelpRequest>(r => r.DreamId == dreamId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Never_accepts_a_dream_id_from_the_caller_only_resolves_the_signed_in_users_own_dream()
    {
        // Same fix, same regression concern as
        // Waypoint.Community.Tests.CreatePostCommandHandlerTests's equivalent test: help requests
        // are visible to mentors specifically to ask for help, which makes accidentally exposing
        // someone else's Dream here even more sensitive than in Community. AttachDream is a bool;
        // there's no code path that accepts a caller-supplied DreamId at all.
        var attackerId = Guid.NewGuid();
        _currentUser.UserId.Returns(attackerId);
        _profileSummaryProvider.GetForUserAsync(attackerId, Arg.Any<CancellationToken>())
            .Returns(new ProfileSummary(attackerId, "Attacker", null));
        _dreamSummaryProvider.GetForUserAsync(attackerId, Arg.Any<CancellationToken>()).Returns((DreamSummary?)null);

        var result = await CreateHandler().Handle(
            new CreateHelpRequestCommand(HelpRequestCategory.Business, "Title", "Body", AttachDream: true),
            CancellationToken.None);

        result.DreamId.Should().BeNull();
        result.AttachedDream.Should().BeNull();
        await _repository.Received(1).AddHelpRequestAsync(
            Arg.Is<HelpRequest>(r => r.DreamId == null), Arg.Any<CancellationToken>());
    }
}
