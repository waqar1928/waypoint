using Microsoft.EntityFrameworkCore;
using Waypoint.AI.Application;
using Waypoint.AI.Domain;

namespace Waypoint.AI.Infrastructure;

public sealed class AiRepository(AiDbContext db) : IAiRepository
{
    public Task<AiConversation?> GetConversationByIdAsync(Guid conversationId, CancellationToken cancellationToken) =>
        db.Conversations.SingleOrDefaultAsync(c => c.Id == conversationId, cancellationToken);

    public async Task<IReadOnlyList<AiConversation>> GetConversationsForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await db.Conversations.Where(c => c.UserId == userId).ToListAsync(cancellationToken);

    public async Task AddConversationAsync(AiConversation conversation, CancellationToken cancellationToken)
    {
        db.Conversations.Add(conversation);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveConversationAsync(AiConversation conversation, CancellationToken cancellationToken)
    {
        if (db.Entry(conversation).State == EntityState.Detached)
        {
            db.Conversations.Update(conversation);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteConversationAsync(Guid conversationId, CancellationToken cancellationToken)
    {
        await db.Conversations.Where(c => c.Id == conversationId).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AiMessage>> GetMessagesForConversationAsync(Guid conversationId, CancellationToken cancellationToken) =>
        await db.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddMessageAsync(AiMessage message, CancellationToken cancellationToken)
    {
        db.Messages.Add(message);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<PromptTemplate?> GetActiveTemplateAsync(string key, CancellationToken cancellationToken) =>
        db.PromptTemplates
            .Where(t => t.Key == key && t.IsActive)
            .OrderByDescending(t => t.Version)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<AiUsageSummaryDto> GetUsageSummaryAsync(CancellationToken cancellationToken)
    {
        var totalConversations = await db.Conversations.CountAsync(cancellationToken);
        var totalMessages = await db.Messages.CountAsync(cancellationToken);
        var totalTokens = await db.Messages.SumAsync(m => (long?)m.TokenCount ?? 0, cancellationToken);

        var conversationCountsByTopic = await db.Conversations
            .GroupBy(c => c.Topic)
            .Select(g => new { Topic = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var messageStatsByTopic = await db.Messages
            .Join(db.Conversations, m => m.ConversationId, c => c.Id, (m, c) => new { c.Topic, m.TokenCount })
            .GroupBy(x => x.Topic)
            .Select(g => new { Topic = g.Key, MessageCount = g.Count(), TotalTokens = g.Sum(x => (long?)x.TokenCount ?? 0) })
            .ToListAsync(cancellationToken);

        var byTopic = conversationCountsByTopic
            .Select(c =>
            {
                var stats = messageStatsByTopic.FirstOrDefault(m => m.Topic == c.Topic);
                return new TopicUsageDto(c.Topic, c.Count, stats?.MessageCount ?? 0, stats?.TotalTokens ?? 0);
            })
            .ToList();

        return new AiUsageSummaryDto(totalConversations, totalMessages, totalTokens, byTopic);
    }
}
