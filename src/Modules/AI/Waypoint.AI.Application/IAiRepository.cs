using Waypoint.AI.Domain;

namespace Waypoint.AI.Application;

public interface IAiRepository
{
    Task<AiConversation?> GetConversationByIdAsync(Guid conversationId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AiConversation>> GetConversationsForUserAsync(Guid userId, CancellationToken cancellationToken);
    Task AddConversationAsync(AiConversation conversation, CancellationToken cancellationToken);
    Task SaveConversationAsync(AiConversation conversation, CancellationToken cancellationToken);

    /// <summary>Cleans up a conversation that was persisted so its Id could be used in the
    /// opening AiRequest, but whose AI call then failed before any message was stored — avoids
    /// leaving an empty, un-openable conversation behind (see StartConversationCommandHandler).</summary>
    Task DeleteConversationAsync(Guid conversationId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AiMessage>> GetMessagesForConversationAsync(Guid conversationId, CancellationToken cancellationToken);
    Task AddMessageAsync(AiMessage message, CancellationToken cancellationToken);

    Task<PromptTemplate?> GetActiveTemplateAsync(string key, CancellationToken cancellationToken);

    /// <summary>Admin-only reporting query (Phase 8 AI usage view) — aggregated in the database,
    /// not loaded into memory, so it stays cheap regardless of message volume.</summary>
    Task<AiUsageSummaryDto> GetUsageSummaryAsync(CancellationToken cancellationToken);
}
