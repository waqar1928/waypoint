using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Waypoint.Dreams.Application;
using Waypoint.Dreams.Application.Admin.GetAllDreams;
using Waypoint.Dreams.Application.GenerateDreamDirections;
using Waypoint.Dreams.Application.GetMyDream;
using Waypoint.Dreams.Application.SelectDreamDirection;
using Waypoint.Dreams.Application.UpdateDreamStatement;

namespace Waypoint.Dreams.Api;

public static class DreamsEndpoints
{
    public static IEndpointRouteBuilder MapDreamsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/dreams").RequireAuthorization().RequireRateLimiting("api").WithTags("Dreams");

        group.MapPost("/directions", async (DiscoveryAnswers answers, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GenerateDreamDirectionsQuery(answers), ct)));

        group.MapPost("/", async (SelectDreamDirectionCommand command, ISender sender, CancellationToken ct) =>
            Results.Created("/api/v1/dreams/me", await sender.Send(command, ct)));

        group.MapGet("/me", async (ISender sender, CancellationToken ct) =>
        {
            var dream = await sender.Send(new GetMyDreamQuery(), ct);
            return dream is null ? Results.NotFound() : Results.Ok(dream);
        });

        group.MapPut("/me", async (UpdateDreamStatementCommand command, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(command, ct)));

        app.MapGroup("/api/v1/admin/dreams")
            .RequireAuthorization("Admin")
            .RequireRateLimiting("api")
            .WithTags("Admin")
            .MapGet("/", async (ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new GetAllDreamsQuery(), ct)));

        return app;
    }
}
