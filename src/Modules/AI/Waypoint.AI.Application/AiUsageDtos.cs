using Waypoint.AI.Domain;

namespace Waypoint.AI.Application;

public sealed record TopicUsageDto(AiConversationTopic Topic, int ConversationCount, int MessageCount, long TotalTokens);

public sealed record AiUsageSummaryDto(
    int TotalConversations, int TotalMessages, long TotalTokens, IReadOnlyList<TopicUsageDto> ByTopic);
