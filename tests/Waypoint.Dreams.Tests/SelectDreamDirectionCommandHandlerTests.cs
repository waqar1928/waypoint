using FluentAssertions;
using MediatR;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Dreams.Application;
using Waypoint.Dreams.Application.SelectDreamDirection;
using Xunit;

namespace Waypoint.Dreams.Tests;

public class SelectDreamDirectionCommandHandlerTests
{
    private readonly IDreamRepository _repository = Substitute.For<IDreamRepository>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();
    private readonly IAuditSink _auditSink = Substitute.For<IAuditSink>();
    private readonly IProductAnalyticsSink _analyticsSink = Substitute.For<IProductAnalyticsSink>();
    private readonly Guid _userId = Guid.NewGuid();

    private SelectDreamDirectionCommandHandler CreateHandler() =>
        new(_repository, _currentUser, _publisher, _auditSink, _analyticsSink);

    private static SelectDreamDirectionCommand BuildCommand() => new(
        "Help small manufacturers cut waste", "Cut waste for small manufacturers",
        null, null, null, null, null, null, IsBusinessShaped: true);

    [Fact]
    public async Task Creates_a_dream_and_publishes_the_onboarding_completed_event()
    {
        _currentUser.UserId.Returns(_userId);
        _repository.ExistsForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns(false);

        var result = await CreateHandler().Handle(BuildCommand(), CancellationToken.None);

        result.Title.Should().Be("Help small manufacturers cut waste");
        result.IsBusinessShaped.Should().BeTrue();
        await _repository.Received(1).SaveAsync(Arg.Any<Waypoint.Dreams.Domain.Dream>(), Arg.Any<CancellationToken>());
        await _publisher.Received(1).Publish(
            Arg.Is<OnboardingCompletedIntegrationEvent>(e => e.UserId == _userId), Arg.Any<CancellationToken>());
        await _auditSink.Received(1).RecordAsync(
            Arg.Is<AuditEntry>(e => e.Action == "Created"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_conflict_when_the_user_already_has_a_dream()
    {
        _currentUser.UserId.Returns(_userId);
        _repository.ExistsForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns(true);

        var act = () => CreateHandler().Handle(BuildCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _repository.DidNotReceive().SaveAsync(Arg.Any<Waypoint.Dreams.Domain.Dream>(), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().Publish(Arg.Any<OnboardingCompletedIntegrationEvent>(), Arg.Any<CancellationToken>());
    }
}
