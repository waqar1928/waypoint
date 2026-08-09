using Waypoint.Common;

namespace Waypoint.AI.Domain;

public sealed class AiMessage : Entity
{
    public Guid ConversationId { get; init; }
    public AiMessageRole Role { get; init; }
    public string Content { get; set; } = string.Empty;
    public string? PromptTemplateVersion { get; set; }
    public int? TokenCount { get; set; }

    public static AiMessage Create(
        Guid conversationId, Guid userId, AiMessageRole role, string content,
        string? promptTemplateVersion, int? tokenCount) =>
        new()
        {
            ConversationId = conversationId,
            Role = role,
            Content = content,
            PromptTemplateVersion = promptTemplateVersion,
            TokenCount = tokenCount,
            CreatedBy = userId,
            UpdatedBy = userId,
        };
}
