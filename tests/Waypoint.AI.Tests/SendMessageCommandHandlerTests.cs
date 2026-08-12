using FluentAssertions;
using NSubstitute;
using Waypoint.AI.Application;
using Waypoint.AI.Application.SendMessage;
using Waypoint.AI.Domain;
using Waypoint.Common;
using Xunit;

namespace Waypoint.AI.Tests;

public class SendMessageCommandHandlerTests
{
    private readonly IAiRepository _repository = Substitute.For<IAiRepository>();
    private readonly IAiService _aiService = Substitute.For<IAiService>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _userId = Guid.NewGuid();

    private SendMessageCommandHandler CreateHandler() => new(_repository, _aiService, _currentUser);

    [Fact]
    public async Task Throws_when_conversation_belongs_to_a_different_user()
    {
        _currentUser.UserId.Returns(_userId);
        var othersConversation = AiConversation.Create(Guid.NewGuid(), null, AiConversationTopic.Coach, null);
        _repository.GetConversationByIdAsync(othersConversation.Id, Arg.Any<CancellationToken>()).Returns(othersConversation);

        var act = () => CreateHandler().Handle(new SendMessageCommand(othersConversation.Id, "Hello"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_when_conversation_does_not_exist()
    {
        _currentUser.UserId.Returns(_userId);
        _repository.GetConversationByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AiConversation?)null);

        var act = () => CreateHandler().Handle(new SendMessageCommand(Guid.NewGuid(), "Hello"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Stores_the_user_message_then_the_ai_reply_and_uses_the_topics_template()
    {
        _currentUser.UserId.Returns(_userId);
        var conversation = AiConversation.Create(_userId, null, AiConversationTopic.ChallengeMyIdea, null);
        _repository.GetConversationByIdAsync(conversation.Id, Arg.Any<CancellationToken>()).Returns(conversation);
        _aiService.CompleteAsync(Arg.Any<AiRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AiResponse("Here's what stands out.", 20, 15, "claude-test", false));

        var result = await CreateHandler().Handle(new SendMessageCommand(conversation.Id, "What do you think?"), CancellationToken.None);

        result.Role.Should().Be(AiMessageRole.Assistant);
        result.Content.Should().Be("Here's what stands out.");

        Received.InOrder(() =>
        {
            _repository.AddMessageAsync(Arg.Is<AiMessage>(m => m.Role == AiMessageRole.User && m.Content == "What do you think?"), Arg.Any<CancellationToken>());
            _repository.AddMessageAsync(Arg.Is<AiMessage>(m => m.Role == AiMessageRole.Assistant), Arg.Any<CancellationToken>());
        });

        await _aiService.Received(1).CompleteAsync(
            Arg.Is<AiRequest>(r => r.PromptTemplateKey == "challenge-my-idea.v1"), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveConversationAsync(conversation, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Regression test for the per-conversation message cap added in the production-readiness
    /// pass (see docs/PRODUCTION_READINESS_AUDIT.md's AI section) — per-minute rate limiting alone
    /// doesn't bound the total size/cost of one long-running conversation, so a hard ceiling exists
    /// as a second, independent control. Must fail fast (no AI call, no message stored) rather than
    /// spending a billed API call and then discovering the conversation is already full.
    /// </summary>
    [Fact]
    public async Task Throws_conflict_and_never_calls_the_ai_service_once_the_conversation_hits_its_message_cap()
    {
        _currentUser.UserId.Returns(_userId);
        var conversation = AiConversation.Create(_userId, null, AiConversationTopic.Coach, null);
        _repository.GetConversationByIdAsync(conversation.Id, Arg.Any<CancellationToken>()).Returns(conversation);
        _repository.GetMessageCountForConversationAsync(conversation.Id, Arg.Any<CancellationToken>()).Returns(100);

        var act = () => CreateHandler().Handle(new SendMessageCommand(conversation.Id, "One more thing"), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _aiService.DidNotReceive().CompleteAsync(Arg.Any<AiRequest>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().AddMessageAsync(Arg.Any<AiMessage>(), Arg.Any<CancellationToken>());
    }
}
