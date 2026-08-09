using FluentAssertions;
using NSubstitute;
using Waypoint.AI.Application;
using Waypoint.AI.Application.StartConversation;
using Waypoint.AI.Domain;
using Waypoint.Common;
using Xunit;

namespace Waypoint.AI.Tests;

public class StartConversationCommandHandlerTests
{
    private readonly IAiRepository _repository = Substitute.For<IAiRepository>();
    private readonly IAiService _aiService = Substitute.For<IAiService>();
    private readonly IDreamSummaryProvider _dreamSummaryProvider = Substitute.For<IDreamSummaryProvider>();
    private readonly IBusinessIdeaSummaryProvider _businessIdeaSummaryProvider = Substitute.For<IBusinessIdeaSummaryProvider>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _dreamId = Guid.NewGuid();

    private StartConversationCommandHandler CreateHandler() =>
        new(_repository, _aiService, _dreamSummaryProvider, _businessIdeaSummaryProvider, _currentUser);

    private void ArrangeSignedInUser(DreamSummary? dream)
    {
        _currentUser.UserId.Returns(_userId);
        _dreamSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns(dream);
    }

    [Fact]
    public async Task Throws_when_dream_analysis_requested_without_a_dream()
    {
        ArrangeSignedInUser(dream: null);

        var act = () => CreateHandler().Handle(new StartConversationCommand(AiConversationTopic.DreamAnalysis), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _repository.DidNotReceive().AddConversationAsync(Arg.Any<AiConversation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_when_challenge_my_idea_requested_without_a_business_idea()
    {
        var dream = new DreamSummary(_dreamId, _userId, "Title", "Statement", null, null, null, null, null, null, true);
        ArrangeSignedInUser(dream);
        _businessIdeaSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns((BusinessIdeaSummary?)null);

        var act = () => CreateHandler().Handle(new StartConversationCommand(AiConversationTopic.ChallengeMyIdea), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Creates_conversation_and_stores_the_ai_opener_for_the_coach_topic()
    {
        var dream = new DreamSummary(_dreamId, _userId, "Cut waste for shops", "Statement", null, null, null, null, null, null, false);
        ArrangeSignedInUser(dream);
        _aiService.CompleteAsync(Arg.Any<AiRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AiResponse("Hi! What's on your mind?", 10, 8, "claude-test", false));

        var result = await CreateHandler().Handle(new StartConversationCommand(AiConversationTopic.Coach), CancellationToken.None);

        result.Topic.Should().Be(AiConversationTopic.Coach);
        result.Messages.Should().ContainSingle(m => m.Role == AiMessageRole.Assistant && m.Content == "Hi! What's on your mind?");

        await _repository.Received(1).AddConversationAsync(
            Arg.Is<AiConversation>(c => c.UserId == _userId && c.DreamId == _dreamId && c.Topic == AiConversationTopic.Coach),
            Arg.Any<CancellationToken>());
        await _aiService.Received(1).CompleteAsync(
            Arg.Is<AiRequest>(r => r.PromptTemplateKey == "coach.v1" && r.Variables.ContainsKey("message")),
            Arg.Any<CancellationToken>());
        await _repository.Received(1).AddMessageAsync(
            Arg.Is<AiMessage>(m => m.Role == AiMessageRole.Assistant), Arg.Any<CancellationToken>());
    }
}
