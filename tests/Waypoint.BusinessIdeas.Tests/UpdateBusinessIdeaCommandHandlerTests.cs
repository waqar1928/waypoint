using FluentAssertions;
using NSubstitute;
using Waypoint.BusinessIdeas.Application;
using Waypoint.BusinessIdeas.Application.UpdateBusinessIdea;
using Waypoint.BusinessIdeas.Domain;
using Waypoint.Common;
using Xunit;

namespace Waypoint.BusinessIdeas.Tests;

public class UpdateBusinessIdeaCommandHandlerTests
{
    private readonly IBusinessIdeasRepository _repository = Substitute.For<IBusinessIdeasRepository>();
    private readonly IDreamSummaryProvider _dreamSummaryProvider = Substitute.For<IDreamSummaryProvider>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _dreamId = Guid.NewGuid();

    private UpdateBusinessIdeaCommandHandler CreateHandler() => new(_repository, _dreamSummaryProvider, _currentUser);

    private static UpdateBusinessIdeaCommand SampleCommand() => new(
        "Problem", "Customer", "Value prop", null, null, null, null, null, null, null, null, null, null, null);

    [Fact]
    public async Task Throws_when_dream_is_not_business_shaped()
    {
        _currentUser.UserId.Returns(_userId);
        _dreamSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new DreamSummary(_dreamId, _userId, "Title", "Statement", null, null, null, null, null, null, IsBusinessShaped: false));

        var act = () => CreateHandler().Handle(SampleCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Creates_a_new_idea_when_none_exists_yet()
    {
        _currentUser.UserId.Returns(_userId);
        _dreamSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new DreamSummary(_dreamId, _userId, "Title", "Statement", null, null, null, null, null, null, IsBusinessShaped: true));
        _repository.GetForDreamAsync(_dreamId, Arg.Any<CancellationToken>()).Returns((BusinessIdea?)null);

        var result = await CreateHandler().Handle(SampleCommand(), CancellationToken.None);

        result.Problem.Should().Be("Problem");
        await _repository.Received(1).AddAsync(Arg.Any<BusinessIdea>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().SaveAsync(Arg.Any<BusinessIdea>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Updates_the_existing_idea_when_one_already_exists()
    {
        _currentUser.UserId.Returns(_userId);
        _dreamSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new DreamSummary(_dreamId, _userId, "Title", "Statement", null, null, null, null, null, null, IsBusinessShaped: true));
        var existing = BusinessIdea.Create(_dreamId, _userId);
        _repository.GetForDreamAsync(_dreamId, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await CreateHandler().Handle(SampleCommand(), CancellationToken.None);

        result.Id.Should().Be(existing.Id);
        await _repository.DidNotReceive().AddAsync(Arg.Any<BusinessIdea>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveAsync(existing, Arg.Any<CancellationToken>());
    }
}
