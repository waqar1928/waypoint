using FluentValidation;
using MediatR;
using Waypoint.AI.Domain;
using Waypoint.Common;

namespace Waypoint.AI.Application.StartConversation;

/// <summary>
/// IncludeProgressContext defaults to false and is entirely opt-in per conversation, not a
/// persistent setting - the user decides fresh each time they start a Coach conversation whether
/// to let it see a snapshot of their current Actions/Experiments alongside their Dream. See
/// StartConversationCommandHandler.BuildProgressContextAsync for what "snapshot" actually means
/// here (bounded, summarized, not a data dump).
/// </summary>
public sealed record StartConversationCommand(AiConversationTopic Topic, bool IncludeProgressContext = false)
    : IRequest<ConversationDto>;

public sealed class StartConversationCommandValidator : AbstractValidator<StartConversationCommand>
{
    public StartConversationCommandValidator()
    {
        RuleFor(x => x.Topic).IsInEnum();
    }
}

public sealed class StartConversationCommandHandler(
    IAiRepository repository,
    IAiService aiService,
    IDreamSummaryProvider dreamSummaryProvider,
    IBusinessIdeaSummaryProvider businessIdeaSummaryProvider,
    IActionsSummaryProvider actionsSummaryProvider,
    IExperimentsSummaryProvider experimentsSummaryProvider,
    IJournalSummaryProvider journalSummaryProvider,
    ICurrentUserAccessor currentUser,
    IProductAnalyticsSink analyticsSink)
    : IRequestHandler<StartConversationCommand, ConversationDto>
{
    private const int MaxOutputTokens = 800;

    public async Task<ConversationDto> Handle(StartConversationCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken);

        var (templateKey, kickoffMessage) = await BuildKickoffAsync(
            request.Topic, request.IncludeProgressContext, dream, userId, cancellationToken);

        var conversation = AiConversation.Create(userId, dream?.DreamId, request.Topic, title: null);
        await repository.AddConversationAsync(conversation, cancellationToken);
        await analyticsSink.TrackAsync(
            new AnalyticsEvent(
                AnalyticsEvents.AiConversationStarted, userId,
                new Dictionary<string, string> { ["topic"] = request.Topic.ToString() },
                DateTimeOffset.UtcNow),
            cancellationToken);

        // The conversation row has to exist before the AI call so its Id is available to pass
        // through — but if the call then fails, clean it back up rather than leaving an empty,
        // message-less conversation sitting in the user's list forever.
        AiResponse response;
        try
        {
            response = await aiService.CompleteAsync(
                new AiRequest(
                    templateKey,
                    new Dictionary<string, string> { ["message"] = kickoffMessage },
                    userId,
                    conversation.Id,
                    MaxOutputTokens),
                cancellationToken);
        }
        catch
        {
            await repository.DeleteConversationAsync(conversation.Id, cancellationToken);
            throw;
        }

        var assistantMessage = AiMessage.Create(
            conversation.Id, userId, AiMessageRole.Assistant, response.Content, templateKey, response.OutputTokens);
        await repository.AddMessageAsync(assistantMessage, cancellationToken);

        return ConversationDto.From(conversation, [MessageDto.From(assistantMessage)]);
    }

    private async Task<(string TemplateKey, string KickoffMessage)> BuildKickoffAsync(
        AiConversationTopic topic, bool includeProgressContext, DreamSummary? dream, Guid userId,
        CancellationToken cancellationToken)
    {
        switch (topic)
        {
            case AiConversationTopic.DreamAnalysis:
                if (dream is null)
                {
                    throw new NotFoundException("Start a Dream before asking Coach to look at it.");
                }
                return ("dream-analysis.v1", BuildDreamKickoff(dream));

            case AiConversationTopic.ChallengeMyIdea:
                var idea = await businessIdeaSummaryProvider.GetForUserAsync(userId, cancellationToken)
                    ?? throw new NotFoundException("Fill in some of your business profile before asking Coach to challenge it.");
                return ("challenge-my-idea.v1", BuildIdeaKickoff(idea));

            default:
                // Progress context (Actions/Experiments) is scoped to the general Coach topic only,
                // not Dream Analysis or Challenge My Idea - both of those already have a single,
                // tight focus (the Dream Statement; the business idea), and mixing in unrelated
                // action/experiment noise would dilute the one thing the user actually asked for.
                // "What am I working on right now" is a general-coaching question, so that's where
                // this belongs.
                var parts = new List<string>
                {
                    dream is not null
                        ? $"Say hello and ask what's on my mind today. My dream so far, in case it's useful: \"{dream.Title}\"."
                        : "Say hello and ask what's on my mind today. I haven't started a Dream yet.",
                };

                if (includeProgressContext)
                {
                    var progressContext = await BuildProgressContextAsync(userId, cancellationToken);
                    if (progressContext is not null)
                    {
                        parts.Add(progressContext);
                    }
                }

                return ("coach.v1", string.Join(" ", parts));
        }
    }

    /// <summary>
    /// Builds the optional "here's what I'm actually working on" context for the general Coach
    /// topic. This lands in the *user* message slot alongside the greeting instruction, same as
    /// the Dream/BusinessIdea context already does — it's the user's own data being summarized
    /// back to Coach, not a system-level instruction, so the existing prompt-injection mitigation
    /// (docs/07-technical-architecture.md: user content only ever goes in the user slot) still
    /// holds. Returns null (rather than an empty string) when there's nothing worth mentioning, so
    /// the caller can skip adding a pointless empty sentence to the kickoff message.
    /// </summary>
    private async Task<string?> BuildProgressContextAsync(Guid userId, CancellationToken cancellationToken)
    {
        var actions = await actionsSummaryProvider.GetForUserAsync(userId, cancellationToken);
        var experiments = await experimentsSummaryProvider.GetForUserAsync(userId, cancellationToken);
        var journal = await journalSummaryProvider.GetForUserAsync(userId, cancellationToken);

        var parts = new List<string>();

        if (actions is { RecentActions.Count: > 0 })
        {
            var actionLines = actions.RecentActions.Select(a =>
                a.IsNextBestAction ? $"\"{a.Title}\" (next best action, {a.Status})" : $"\"{a.Title}\" ({a.Status})");
            parts.Add($"Here's what I'm actually working on right now: {string.Join("; ", actionLines)}.");
        }

        if (experiments is { RecentExperiments.Count: > 0 })
        {
            // LatestLearning used to be fetched here and silently dropped - the experiment line
            // mentioned the outcome (validated/invalidated/etc.) but never what was actually
            // learned, which is the part a coaching conversation can actually use.
            var experimentLines = experiments.RecentExperiments.Select(e =>
                (e.LatestOutcome, e.LatestLearning) switch
                {
                    (not null, not null) => $"\"{e.IdeaDescription}\" ({e.Status}, result so far: {e.LatestOutcome} - learned: {e.LatestLearning})",
                    (not null, null) => $"\"{e.IdeaDescription}\" ({e.Status}, result so far: {e.LatestOutcome})",
                    _ => $"\"{e.IdeaDescription}\" ({e.Status})",
                });
            parts.Add($"My recent experiments: {string.Join("; ", experimentLines)}.");
        }

        if (journal is { RecentLessons.Count: > 0 })
        {
            var lessonLines = journal.RecentLessons.Select(l => $"\"{l.Body}\"");
            parts.Add($"Some things I've learned along the way: {string.Join("; ", lessonLines)}.");
        }

        return parts.Count > 0 ? string.Join(" ", parts) : null;
    }

    private static string BuildDreamKickoff(DreamSummary dream)
    {
        var parts = new List<string>
        {
            $"Here's my Dream Statement so far. Title: \"{dream.Title}\". Statement: \"{dream.Statement}\".",
        };
        if (!string.IsNullOrWhiteSpace(dream.Purpose)) parts.Add($"Purpose: \"{dream.Purpose}\".");
        if (!string.IsNullOrWhiteSpace(dream.WhoItHelps)) parts.Add($"Who it helps: \"{dream.WhoItHelps}\".");
        if (!string.IsNullOrWhiteSpace(dream.Problem)) parts.Add($"Problem: \"{dream.Problem}\".");
        parts.Add("Look at this with me. What stands out, what's strong, and what's worth pushing on?");
        return string.Join(" ", parts);
    }

    private static string BuildIdeaKickoff(BusinessIdeaSummary idea)
    {
        var parts = new List<string> { "Here's my business idea so far." };
        if (!string.IsNullOrWhiteSpace(idea.Problem)) parts.Add($"Problem: \"{idea.Problem}\".");
        if (!string.IsNullOrWhiteSpace(idea.Customer)) parts.Add($"Customer: \"{idea.Customer}\".");
        if (!string.IsNullOrWhiteSpace(idea.ValueProposition)) parts.Add($"Value proposition: \"{idea.ValueProposition}\".");
        if (!string.IsNullOrWhiteSpace(idea.Pricing)) parts.Add($"Pricing: \"{idea.Pricing}\".");
        if (!string.IsNullOrWhiteSpace(idea.Competitors)) parts.Add($"Competitors: \"{idea.Competitors}\".");
        parts.Add("Challenge this. Where's it weakest, and what am I assuming that might not be true?");
        return string.Join(" ", parts);
    }
}
