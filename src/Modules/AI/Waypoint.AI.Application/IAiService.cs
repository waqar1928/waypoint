namespace Waypoint.AI.Application;

/// <summary>
/// Application-layer port for AI completions (see docs/07-technical-architecture.md "AI
/// architecture"). No application code depends on a specific vendor SDK — the concrete adapter
/// (AnthropicAiService, etc.) lives in Waypoint.AI.Infrastructure and is selected via DI.
/// </summary>
public interface IAiService
{
    Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// PromptTemplateKey identifies a versioned, data-driven prompt row (see PromptTemplate).
/// Variables always includes "message" — either the real text the user typed, or, for a
/// conversation's opening turn, a synthetic kickoff instruction built from Dream/BusinessIdea
/// context. User-authored content is never concatenated into the system prompt (prompt-injection
/// mitigation per docs/07 lines 144-149) — it only ever flows through this Variables dictionary
/// into the user-message slot of the underlying chat API call.
/// </summary>
public sealed record AiRequest(
    string PromptTemplateKey,
    IReadOnlyDictionary<string, string> Variables,
    Guid UserId,
    Guid ConversationId,
    int MaxOutputTokens);

public sealed record AiResponse(
    string Content,
    int InputTokens,
    int OutputTokens,
    string ModelId,
    bool WasModerationFlagged);
