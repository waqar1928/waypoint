using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Waypoint.Goals.Application.GeneratePlanDraft;
using Waypoint.Goals.Application.GetMyPlan;
using Waypoint.Goals.Application.Milestones;
using Waypoint.Goals.Application.SavePlan;
using Waypoint.Goals.Application.UpdateGoal;
using Waypoint.Goals.Application.UpdateMission;

namespace Waypoint.Goals.Api;

public static class GoalsEndpoints
{
    public static IEndpointRouteBuilder MapGoalsEndpoints(this IEndpointRouteBuilder app)
    {
        var plan = app.MapGroup("/api/v1/plan").RequireAuthorization().RequireRateLimiting("api").WithTags("Plan");

        plan.MapGet("/draft", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GeneratePlanDraftQuery(), ct)));

        plan.MapPost("/", async (SavePlanCommand command, ISender sender, CancellationToken ct) =>
            Results.Created("/api/v1/plan/me", await sender.Send(command, ct)));

        plan.MapGet("/me", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetMyPlanQuery(), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        plan.MapPut("/goals/{goalId:guid}", async (Guid goalId, UpdateGoalRequest body, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new UpdateGoalCommand(goalId, body.Statement, body.TargetDate), ct)));

        plan.MapPut("/missions/{missionId:guid}", async (Guid missionId, UpdateMissionRequest body, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new UpdateMissionCommand(missionId, body.Title, body.TargetDate), ct)));

        var milestones = app.MapGroup("/api/v1/milestones").RequireAuthorization().RequireRateLimiting("api").WithTags("Milestones");

        milestones.MapGet("/", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetMyMilestonesQuery(), ct)));

        milestones.MapPost("/", async (CreateMilestoneCommand command, ISender sender, CancellationToken ct) =>
            Results.Created("/api/v1/milestones", await sender.Send(command, ct)));

        milestones.MapPost("/{milestoneId:guid}/achieve", async (Guid milestoneId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new MarkMilestoneAchievedCommand(milestoneId), ct)));

        return app;
    }
}

public sealed record UpdateGoalRequest(string Statement, DateOnly? TargetDate);
public sealed record UpdateMissionRequest(string Title, DateOnly? TargetDate);
