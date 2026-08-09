using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Waypoint.Experiments.Application.CreateExperiment;
using Waypoint.Experiments.Application.GetMyExperiments;
using Waypoint.Experiments.Application.RecordExperimentResult;
using Waypoint.Experiments.Application.UpdateExperimentStatus;
using Waypoint.Experiments.Domain;

namespace Waypoint.Experiments.Api;

public static class ExperimentsEndpoints
{
    public static IEndpointRouteBuilder MapExperimentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/experiments").RequireAuthorization().RequireRateLimiting("api").WithTags("Experiments");

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetMyExperimentsQuery(), ct)));

        group.MapPost("/", async (CreateExperimentCommand command, ISender sender, CancellationToken ct) =>
            Results.Created("/api/v1/experiments", await sender.Send(command, ct)));

        group.MapPut("/{experimentId:guid}/status", async (Guid experimentId, UpdateExperimentStatusRequest body, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new UpdateExperimentStatusCommand(experimentId, body.Status), ct)));

        group.MapPost("/{experimentId:guid}/results", async (Guid experimentId, RecordExperimentResultRequest body, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new RecordExperimentResultCommand(experimentId, body.Outcome, body.Evidence, body.Learning), ct)));

        return app;
    }
}

public sealed record UpdateExperimentStatusRequest(ExperimentStatus Status);
public sealed record RecordExperimentResultRequest(ExperimentOutcome Outcome, string? Evidence, string Learning);
