namespace Waypoint.AI.Domain;

/// <summary>
/// What a conversation with Waypoint Coach is about. Idea Studio is deferred (no seed content or
/// UI spec exists yet — see docs/09-phased-plan.md Phase 6 scope vs. what's actually written up).
/// </summary>
public enum AiConversationTopic { Coach, DreamAnalysis, ChallengeMyIdea }

public enum AiMessageRole { User, Assistant, System }
