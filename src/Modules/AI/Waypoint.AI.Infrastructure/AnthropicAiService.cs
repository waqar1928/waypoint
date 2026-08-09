using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
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
    HttpClient httpClient, AiDbContext db, IConfiguration configuration, ILogger<AnthropicAiService> logger)
    : IAiService
{
    private const string DefaultModel = "claude-sonnet-4-5-20250929";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken cancellationToken)
    {
        var template = await db.PromptTemplates
            .Where(t => t.Key == request.PromptTemplateKey && t.IsActive)
            .OrderByDescending(t => t.Version)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new AiServiceUnavailableException($"No active prompt template for key '{request.PromptTemplateKey}'.");

        var apiKey = configuration["ANTHROPIC_API_KEY"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new AiServiceUnavailableException(
                "Waypoint Coach isn't configured yet — the ANTHROPIC_API_KEY environment variable is missing.");
        }

        var history = await db.Messages
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
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Anthropic API call failed for conversation {ConversationId}", request.ConversationId);
            throw new AiServiceUnavailableException("Waypoint Coach couldn't reach the AI service right now. Please try again.");
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            var body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Anthropic API returned {Status}: {Body}", httpResponse.StatusCode, body);
            throw new AiServiceUnavailableException("Waypoint Coach couldn't get a response right now. Please try again.");
        }

        var result = await httpResponse.Content.ReadFromJsonAsync<AnthropicResponse>(JsonOptions, cancellationToken)
            ?? throw new AiServiceUnavailableException("Waypoint Coach returned an unreadable response.");

        var text = string.Join("\n\n", result.Content.Where(c => c.Type == "text").Select(c => c.Text));

        // Basic output moderation pass (docs/07 lines 137-139): today this only checks for
        // triggering an empty/blocked response; a real moderation API call is a natural upgrade
        // here without touching any calling code, since WasModerationFlagged is already part of
        // the AiResponse contract.
        var wasModerationFlagged = string.IsNullOrWhiteSpace(text);

        return new AiResponse(
            wasModerationFlagged ? "Coach didn't have a response for that — try rephrasing?" : text,
            result.Usage.InputTokens,
            result.Usage.OutputTokens,
            result.Model,
            wasModerationFlagged);
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
