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
    private readonly IActionsSummaryProvider _actionsSummaryProvider = Substitute.For<IActionsSummaryProvider>();
    private readonly IExperimentsSummaryProvider _experimentsSummaryProvider = Substitute.For<IExperimentsSummaryProvider>();
    private readonly IJournalSummaryProvider _journalSummaryProvider = Substitute.For<IJournalSummaryProvider>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly IProductAnalyticsSink _analyticsSink = Substitute.For<IProductAnalyticsSink>();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _dreamId = Guid.NewGuid();

    private StartConversationCommandHandler CreateHandler() =>
        new(_repository, _aiService, _dreamSummaryProvider, _businessIdeaSummaryProvider,
            _actionsSummaryProvider, _experimentsSummaryProvider, _journalSummaryProvider, _currentUser, _analyticsSink);

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

    [Fact]
    public async Task Does_not_call_the_progress_summary_providers_when_IncludeProgressContext_is_false()
    {
        var dream = new DreamSummary(_dreamId, _userId, "Cut waste for shops", "Statement", null, null, null, null, null, null, false);
        ArrangeSignedInUser(dream);
        _aiService.CompleteAsync(Arg.Any<AiRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AiResponse("Hi!", 10, 8, "claude-test", false));

        await CreateHandler().Handle(new StartConversationCommand(AiConversationTopic.Coach), CancellationToken.None);

        // IncludeProgressContext defaults to false - the whole point of this being opt-in is that
        // a normal conversation start never touches Actions/Experiments/Journal data at all, not
        // just that it's excluded from the prompt.
        await _actionsSummaryProvider.DidNotReceive().GetForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _experimentsSummaryProvider.DidNotReceive().GetForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _journalSummaryProvider.DidNotReceive().GetForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Includes_actions_experiments_and_learnings_in_the_kickoff_message_when_opted_in()
    {
        var dream = new DreamSummary(_dreamId, _userId, "Cut waste for shops", "Statement", null, null, null, null, null, null, false);
        ArrangeSignedInUser(dream);
        _actionsSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns(
            new ActionsSummary([new ActionSummaryItem("Call three shop owners", "InProgress", IsNextBestAction: true)]));
        _experimentsSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns(
            new ExperimentsSummary([new ExperimentSummaryItem("Post in a local business group", "Completed", "Validated", "People are interested")]));
        _journalSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns(
            new JournalSummary([new LessonSummaryItem("Speed matters more than price", DateTimeOffset.UtcNow)]));
        _aiService.CompleteAsync(Arg.Any<AiRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AiResponse("Hi!", 10, 8, "claude-test", false));

        await CreateHandler().Handle(
            new StartConversationCommand(AiConversationTopic.Coach, IncludeProgressContext: true), CancellationToken.None);

        await _aiService.Received(1).CompleteAsync(
            Arg.Is<AiRequest>(r =>
                r.Variables["message"].Contains("Call three shop owners") &&
                r.Variables["message"].Contains("next best action") &&
                r.Variables["message"].Contains("Post in a local business group") &&
                r.Variables["message"].Contains("Validated") &&
                // The experiment's LatestLearning used to be fetched and silently dropped before
                // this was fixed - "People are interested" is that field, and must now reach the
                // model, not just the outcome label.
                r.Variables["message"].Contains("People are interested") &&
                r.Variables["message"].Contains("Speed matters more than price")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Does_not_add_an_empty_progress_sentence_when_opted_in_but_nothing_exists_yet()
    {
        var dream = new DreamSummary(_dreamId, _userId, "Cut waste for shops", "Statement", null, null, null, null, null, null, false);
        ArrangeSignedInUser(dream);
        _actionsSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns(new ActionsSummary([]));
        _experimentsSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns(new ExperimentsSummary([]));
        _aiService.CompleteAsync(Arg.Any<AiRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AiResponse("Hi!", 10, 8, "claude-test", false));

        await CreateHandler().Handle(
            new StartConversationCommand(AiConversationTopic.Coach, IncludeProgressContext: true), CancellationToken.None);

        await _aiService.Received(1).CompleteAsync(
            Arg.Is<AiRequest>(r => r.Variables["message"] == "Say hello and ask what's on my mind today. My dream so far, in case it's useful: \"Cut waste for shops\"."),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Does_not_include_progress_context_for_the_dream_analysis_topic_even_when_opted_in()
    {
        var dream = new DreamSummary(_dreamId, _userId, "Cut waste for shops", "Statement", null, null, null, null, null, null, false);
        ArrangeSignedInUser(dream);
        _aiService.CompleteAsync(Arg.Any<AiRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AiResponse("Hi!", 10, 8, "claude-test", false));

        await CreateHandler().Handle(
            new StartConversationCommand(AiConversationTopic.DreamAnalysis, IncludeProgressContext: true), CancellationToken.None);

        // Progress context is deliberately scoped to the general Coach topic only - see the doc
        // comment on BuildKickoffAsync's default case.
        await _actionsSummaryProvider.DidNotReceive().GetForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _experimentsSummaryProvider.DidNotReceive().GetForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
