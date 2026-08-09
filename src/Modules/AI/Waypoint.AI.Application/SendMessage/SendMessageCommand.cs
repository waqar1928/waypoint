using FluentValidation;
using MediatR;
using Waypoint.AI.Domain;
using Waypoint.Common;

namespace Waypoint.AI.Application.SendMessage;

public sealed record SendMessageCommand(Guid ConversationId, string Content) : IRequest<MessageDto>;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty().MaximumLength(4000);
    }
}

public sealed class SendMessageCommandHandler(IAiRepository repository, IAiService aiService, ICurrentUserAccessor currentUser)
    : IRequestHandler<SendMessageCommand, MessageDto>
{
    private const int MaxOutputTokens = 800;

    private static readonly Dictionary<AiConversationTopic, string> ReplyTemplateKeys = new()
    {
        [AiConversationTopic.Coach] = "coach.v1",
        [AiConversationTopic.DreamAnalysis] = "dream-analysis.v1",
        [AiConversationTopic.ChallengeMyIdea] = "challenge-my-idea.v1",
    };

    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var conversation = await repository.GetConversationByIdAsync(request.ConversationId, cancellationToken);
        if (conversation is null || conversation.UserId != userId)
        {
            throw new NotFoundException("Conversation not found.");
        }

        var userMessage = AiMessage.Create(conversation.Id, userId, AiMessageRole.User, request.Content, null, null);
        await repository.AddMessageAsync(userMessage, cancellationToken);

        var templateKey = ReplyTemplateKeys[conversation.Topic];
        var response = await aiService.CompleteAsync(
            new AiRequest(
                templateKey,
                new Dictionary<string, string> { ["message"] = request.Content },
                userId,
                conversation.Id,
                MaxOutputTokens),
            cancellationToken);

        var assistantMessage = AiMessage.Create(
            conversation.Id, userId, AiMessageRole.Assistant, response.Content, templateKey, response.OutputTokens);
        await repository.AddMessageAsync(assistantMessage, cancellationToken);

        conversation.UpdatedBy = userId;
        await repository.SaveConversationAsync(conversation, cancellationToken);

        return MessageDto.From(assistantMessage);
    }
}
