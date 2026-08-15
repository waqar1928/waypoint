using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Waypoint.AI.Application.GetAiUsageSummary;
using Waypoint.AI.Application.GetConversationMessages;
using Waypoint.AI.Application.GetMyConversations;
using Waypoint.AI.Application.SendMessage;
using Waypoint.AI.Application.StartConversation;
using Waypoint.AI.Domain;

namespace Waypoint.AI.Api;

public static class AiEndpoints
{
    public static IEndpointRouteBuilder MapAiEndpoints(this IEndpointRouteBuilder app)
    {
        // Split by cost, not by resource: only the two routes that actually trigger a billed
        // Anthropic completion (StartConversation's opening turn, SendMessage) sit under the
        // strict "ai" policy. Plain reads (list conversations, load message history) are no
        // costlier than any other DB read, so they share the standard "api" budget — putting them
        // under "ai" too would let a single coach-page load's list+messages fetch eat into the
        // same 20/min budget as real AI turns, defeating the point of a separate, stricter tier.
        var group = app.MapGroup("/api/v1/ai").RequireAuthorization().WithTags("AI");

        group.MapGet("/conversations", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetMyConversationsQuery(), ct)))
            .RequireRateLimiting("api");

        group.MapPost("/conversations", async (StartConversationRequest body, ISender sender, CancellationToken ct) =>
            Results.Created(
                "/api/v1/ai/conversations",
                await sender.Send(new StartConversationCommand(body.Topic, body.IncludeProgressContext), ct)))
            .RequireRateLimiting("ai");

        group.MapGet("/conversations/{conversationId:guid}/messages", async (Guid conversationId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetConversationMessagesQuery(conversationId), ct)))
            .RequireRateLimiting("api");

        group.MapPost("/conversations/{conversationId:guid}/messages", async (Guid conversationId, SendMessageRequest body, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new SendMessageCommand(conversationId, body.Content), ct)))
            .RequireRateLimiting("ai");

        app.MapGroup("/api/v1/admin/ai-usage")
            .RequireAuthorization("Admin")
            .RequireRateLimiting("api")
            .WithTags("Admin")
            .MapGet("/", async (ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetAiUsageSummaryQuery(), ct)));

        return app;
    }
}

public sealed record StartConversationRequest(AiConversationTopic Topic, bool IncludeProgressContext = false);
public sealed record SendMessageRequest(string Content);
