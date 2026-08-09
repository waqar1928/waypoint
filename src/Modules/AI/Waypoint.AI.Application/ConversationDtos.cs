using Waypoint.AI.Domain;

namespace Waypoint.AI.Application;

public sealed record MessageDto(Guid Id, AiMessageRole Role, string Content, DateTimeOffset CreatedAt)
{
    public static MessageDto From(AiMessage m) => new(m.Id, m.Role, m.Content, m.CreatedAt);
}

public sealed record ConversationDto(Guid Id, AiConversationTopic Topic, Guid? DreamId, IReadOnlyList<MessageDto> Messages)
{
    public static ConversationDto From(AiConversation c, IReadOnlyList<MessageDto> messages) =>
        new(c.Id, c.Topic, c.DreamId, messages);
}

public sealed record ConversationSummaryDto(Guid Id, AiConversationTopic Topic, DateTimeOffset UpdatedAt)
{
    public static ConversationSummaryDto From(AiConversation c) => new(c.Id, c.Topic, c.UpdatedAt);
}
