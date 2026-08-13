using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Waypoint.AI.Application;
using Waypoint.AI.Domain;
using Waypoint.Common;

namespace Waypoint.AI.Infrastructure;

/// <summary>
/// Concrete IAiService adapter calling Anthropic's Messages API. No other layer in the codebase
/// references Anthropic types or endpoints directly (docs/07-technical-architecture.md) — swapping
/// providers means writing a new class here and changing one DI registration.
///
/// Prompt-injection mitigation (docs/07 lines 144-149): the user's real text only ever lands in a
/// "user" message slot, never appended to the system prompt. The system prompt is fixed,
/// versioned, data-driven content from PromptTemplate — the model is told, in that system prompt,
/// to treat user content as information rather than instructions.
/// </summary>
public sealed class AnthropicAiService(
    HttpClient httpClient, AiDbContext db, IConfiguration configuration, IMemoryCache cache, ILogger<AnthropicAiService> logger)
    : IAiService
{
    private const string DefaultModel = "claude-sonnet-4-5-20250929";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken cancellationToken)
    {
        // PromptTemplate is global reference data (same active version for a given key regardless
        // of who's asking) seeded once at startup with no runtime edit path yet — a genuinely safe
        // cache target, unlike almost everything else in this app which is per-user. Queried on
        // every single AI turn (this method is the hot path for both StartConversation's opening
        // turn and every SendMessage call), so this removes a DB round-trip from every AI request.
        var cacheKey = $"prompt-template:{request.PromptTemplateKey}";
        var template = await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(10);
            return await db.PromptTemplates
                .AsNoTracking()
                .Where(t => t.Key == request.PromptTemplateKey && t.IsActive)
                .OrderByDescending(t => t.Version)
                .FirstOrDefaultAsync(cancellationToken);
        }) ?? throw new AiServiceUnavailableException($"No active prompt template for key '{request.PromptTemplateKey}'.");

        var apiKey = configuration["ANTHROPIC_API_KEY"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new AiServiceUnavailableException(
                "Drevia Coach isn't configured yet. The ANTHROPIC_API_KEY environment variable is missing.");
        }

        var history = await db.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == request.ConversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        var messages = new List<AnthropicMessage>();
        if (history.Count == 0)
        {
            // Opening turn — nothing stored yet, so the sole "user" message is the synthetic
            // kickoff instruction the Application layer built from Dream/BusinessIdea context.
            messages.Add(new AnthropicMessage("user", ApplyPlaceholders(template.UserPromptFormat, request.Variables)));
        }
        else
        {
            messages.AddRange(history.Select(m =>
                new AnthropicMessage(m.Role == AiMessageRole.Assistant ? "assistant" : "user", m.Content)));
        }

        var model = configuration["Waypoint:AI:Model"] ?? DefaultModel;
        var payload = new AnthropicRequest(
            model,
            request.MaxOutputTokens,
            ApplyPlaceholders(template.SystemPrompt, request.Variables),
            messages);

        var httpResponse = await SendWithRetryAsync(payload, apiKey, request.ConversationId, cancellationToken);

        var result = await httpResponse.Content.ReadFromJsonAsync<AnthropicResponse>(JsonOptions, cancellationToken)
            ?? throw new AiServiceUnavailableException("Drevia Coach returned an unreadable response.");

        var text = string.Join("\n\n", result.Content.Where(c => c.Type == "text").Select(c => c.Text));

        // Basic output moderation pass (docs/07 lines 137-139): today this only checks for
        // triggering an empty/blocked response; a real moderation API call is a natural upgrade
        // here without touching any calling code, since WasModerationFlagged is already part of
        // the AiResponse contract.
        var wasModerationFlagged = string.IsNullOrWhiteSpace(text);

        return new AiResponse(
            wasModerationFlagged ? "Coach didn't have a response for that. Try rephrasing?" : text,
            result.Usage.InputTokens,
            result.Usage.OutputTokens,
            result.Model,
            wasModerationFlagged);
    }

    // Bounded retry for transient failures only (network errors and 5xx) — never retries a 4xx,
    // since that's the API telling us the request itself is wrong and retrying it would just fail
    // the same way again. See docs/PRODUCTION_READINESS_AUDIT.md's AI/Security sections: this was
    // a real gap (a single transient blip failed the whole AI turn immediately) that a hand-rolled
    // bounded retry closes without pulling in a new dependency (Polly) for one call site.
    private const int MaxAttempts = 3;
    private static readonly TimeSpan[] RetryDelays = [TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(1500)];

    private async Task<HttpResponseMessage> SendWithRetryAsync(
        AnthropicRequest payload, string apiKey, Guid conversationId, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            // HttpRequestMessage can only be sent once, so this has to be rebuilt every attempt
            // rather than hoisted above the loop.
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "v1/messages")
            {
                Content = JsonContent.Create(payload, options: JsonOptions),
            };
            httpRequest.Headers.Add("x-api-key", apiKey);
            httpRequest.Headers.Add("anthropic-version", "2023-06-01");

            HttpResponseMessage httpResponse;
            try
            {
                httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
            }
            catch (HttpRequestException ex) when (attempt < MaxAttempts)
            {
                logger.LogWarning(ex,
                    "Anthropic API call failed for conversation {ConversationId} (attempt {Attempt}/{MaxAttempts}), retrying",
                    conversationId, attempt, MaxAttempts);
                await Task.Delay(RetryDelays[attempt - 1], cancellationToken);
                continue;
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Anthropic API call failed for conversation {ConversationId} after {MaxAttempts} attempts",
                    conversationId, MaxAttempts);
                throw new AiServiceUnavailableException("Drevia Coach couldn't reach the AI service right now. Please try again.");
            }

            if (httpResponse.IsSuccessStatusCode)
            {
                return httpResponse;
            }

            // 5xx is the API's own infrastructure having a bad moment — worth retrying. Anything
            // else (400 bad request, 401 auth, 429 rate limit) reflects something about this
            // specific request/account that a retry won't fix, so fail immediately instead of
            // burning two more attempts (and, for 429, making the rate-limit situation worse).
            var isRetryableStatus = (int)httpResponse.StatusCode >= 500;
            if (isRetryableStatus && attempt < MaxAttempts)
            {
                logger.LogWarning(
                    "Anthropic API returned {Status} for conversation {ConversationId} (attempt {Attempt}/{MaxAttempts}), retrying",
                    httpResponse.StatusCode, conversationId, attempt, MaxAttempts);
                httpResponse.Dispose();
                await Task.Delay(RetryDelays[attempt - 1], cancellationToken);
                continue;
            }

            var body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Anthropic API returned {Status}: {Body}", httpResponse.StatusCode, body);
            throw new AiServiceUnavailableException("Drevia Coach couldn't get a response right now. Please try again.");
        }

        // Unreachable — the loop above always either returns or throws on its final iteration —
        // but the compiler can't prove that, so this satisfies the "all paths return" requirement.
        throw new AiServiceUnavailableException("Drevia Coach couldn't get a response right now. Please try again.");
    }

    private static string ApplyPlaceholders(string template, IReadOnlyDictionary<string, string> variables)
    {
        var result = template;
        foreach (var (key, value) in variables)
        {
            result = result.Replace("{{" + key + "}}", value);
        }
        return result;
    }

    private sealed record AnthropicMessage(string Role, string Content);

    private sealed record AnthropicRequest(
        string Model,
        [property: JsonPropertyName("max_tokens")] int MaxTokens,
        string System,
        List<AnthropicMessage> Messages);

    private sealed record AnthropicResponse(string Model, List<AnthropicContentBlock> Content, AnthropicUsage Usage);

    private sealed record AnthropicContentBlock(string Type, string Text);

    private sealed record AnthropicUsage(
        [property: JsonPropertyName("input_tokens")] int InputTokens,
        [property: JsonPropertyName("output_tokens")] int OutputTokens);
}
