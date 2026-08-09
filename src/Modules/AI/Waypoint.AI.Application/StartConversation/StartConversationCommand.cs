using FluentValidation;
using MediatR;
using Waypoint.AI.Domain;
using Waypoint.Common;

namespace Waypoint.AI.Application.StartConversation;

public sealed record StartConversationCommand(AiConversationTopic Topic) : IRequest<ConversationDto>;

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
    ICurrentUserAccessor currentUser)
    : IRequestHandler<StartConversationCommand, ConversationDto>
{
    private const int MaxOutputTokens = 800;

    public async Task<ConversationDto> Handle(StartConversationCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken);

        var (templateKey, kickoffMessage) = await BuildKickoffAsync(request.Topic, dream, userId, cancellationToken);

        var conversation = AiConversation.Create(userId, dream?.DreamId, request.Topic, title: null);
        await repository.AddConversationAsync(conversation, cancellationToken);

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
        AiConversationTopic topic, DreamSummary? dream, Guid userId, CancellationToken cancellationToken)
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
                var greeting = dream is not null
                    ? $"Say hello and ask what's on my mind today. My dream so far, in case it's useful: \"{dream.Title}\"."
                    : "Say hello and ask what's on my mind today. I haven't started a Dream yet.";
                return ("coach.v1", greeting);
        }
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
        parts.Add("Look at this with me — what stands out, what's strong, and what's worth pushing on?");
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
        parts.Add("Challenge this — where's it weakest, and what am I assuming that might not be true?");
        return string.Join(" ", parts);
    }
}
