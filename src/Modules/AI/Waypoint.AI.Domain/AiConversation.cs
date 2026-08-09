using Waypoint.Common;

namespace Waypoint.AI.Domain;

/// <summary>
/// A conversation thread with Waypoint Coach. dream_id is nullable — a general coaching chat
/// doesn't require a Dream to exist yet — but dream_analysis and challenge_my_idea conversations
/// always carry one, since they're seeded from that context.
/// </summary>
public sealed class AiConversation : Entity
{
    public Guid UserId { get; init; }
    public Guid? DreamId { get; init; }
    public AiConversationTopic Topic { get; init; }
    public string? Title { get; set; }

    public static AiConversation Create(Guid userId, Guid? dreamId, AiConversationTopic topic, string? title) =>
        new()
        {
            UserId = userId,
            DreamId = dreamId,
            Topic = topic,
            Title = title,
            CreatedBy = userId,
            UpdatedBy = userId,
        };
}
