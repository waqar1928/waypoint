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
        var group = app.MapGroup("/api/v1/ai").RequireAuthorization().RequireRateLimiting("ai").WithTags("AI");

        group.MapGet("/conversations", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetMyConversationsQuery(), ct)));

        group.MapPost("/conversations", async (StartConversationRequest body, ISender sender, CancellationToken ct) =>
            Results.Created("/api/v1/ai/conversations", await sender.Send(new StartConversationCommand(body.Topic), ct)));

        group.MapGet("/conversations/{conversationId:guid}/messages", async (Guid conversationId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetConversationMessagesQuery(conversationId), ct)));

        group.MapPost("/conversations/{conversationId:guid}/messages", async (Guid conversationId, SendMessageRequest body, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new SendMessageCommand(conversationId, body.Content), ct)));

        app.MapGroup("/api/v1/admin/ai-usage")
            .RequireAuthorization("Admin")
            .RequireRateLimiting("api")
            .WithTags("Admin")
            .MapGet("/", async (ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetAiUsageSummaryQuery(), ct)));

        return app;
    }
}

public sealed record StartConversationRequest(AiConversationTopic Topic);
public sealed record SendMessageRequest(string Content);
