using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Waypoint.Actions.Application.AddActionReflection;
using Waypoint.Actions.Application.CreateAction;
using Waypoint.Actions.Application.GetMyActions;
using Waypoint.Actions.Application.GetNextBestAction;
using Waypoint.Actions.Application.SetNextBestAction;
using Waypoint.Actions.Application.UpdateAction;
using Waypoint.Actions.Application.UpdateActionStatus;
using Waypoint.Actions.Domain;

namespace Waypoint.Actions.Api;

public static class ActionsEndpoints
{
    public static IEndpointRouteBuilder MapActionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/actions").RequireAuthorization().RequireRateLimiting("api").WithTags("Actions");

        group.MapGet("/", async (ActionStatus? status, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetMyActionsQuery(status), ct)));

        group.MapGet("/next-best", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetNextBestActionQuery(), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapPost("/", async (CreateActionCommand command, ISender sender, CancellationToken ct) =>
            Results.Created("/api/v1/actions", await sender.Send(command, ct)));

        group.MapPut("/{actionId:guid}", async (Guid actionId, UpdateActionRequest body, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(
                new UpdateActionCommand(actionId, body.Title, body.Description, body.Priority, body.Difficulty,
                    body.EstimatedMinutes, body.ExpectedImpact, body.DueDate), ct)));

        group.MapPut("/{actionId:guid}/status", async (Guid actionId, UpdateActionStatusRequest body, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new UpdateActionStatusCommand(actionId, body.Status), ct)));

        group.MapPost("/{actionId:guid}/set-next-best", async (Guid actionId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new SetNextBestActionCommand(actionId), ct)));

        group.MapPost("/{actionId:guid}/reflection", async (Guid actionId, AddActionReflectionRequest body, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new AddActionReflectionCommand(actionId, body.WhatHappened, body.Learning), ct);
            return Results.NoContent();
        });

        return app;
    }
}

public sealed record UpdateActionRequest(
    string Title, string? Description, ActionPriority Priority, ActionDifficulty Difficulty,
    int? EstimatedMinutes, ActionImpact ExpectedImpact, DateOnly? DueDate);

public sealed record UpdateActionStatusRequest(ActionStatus Status);
public sealed record AddActionReflectionRequest(string? WhatHappened, string? Learning);
