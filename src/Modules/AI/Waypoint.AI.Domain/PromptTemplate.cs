using Waypoint.Common;

namespace Waypoint.AI.Domain;

/// <summary>
/// Prompt templates are data, not code (see docs/07-technical-architecture.md "AI architecture")
/// — every coaching prompt is original content authored for Waypoint, stored as a versioned row
/// so it can be reviewed and changed without a deploy, never sourced from or imitating a specific
/// author's voice (docs/01-product-requirements.md §9, guardrail #3).
///
/// SystemPrompt and UserPromptFormat may contain {{placeholder}} tokens substituted from an
/// AiRequest's Variables at call time.
/// </summary>
public sealed class PromptTemplate : Entity
{
    public string Key { get; init; } = string.Empty;
    public int Version { get; init; }
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPromptFormat { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public static PromptTemplate Create(string key, int version, string systemPrompt, string userPromptFormat) =>
        new()
        {
            Key = key,
            Version = version,
            SystemPrompt = systemPrompt,
            UserPromptFormat = userPromptFormat,
            IsActive = true,
        };
}
