using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Waypoint.BusinessIdeas.Application.GenerateBusinessValidation;
using Waypoint.BusinessIdeas.Application.GetMyBusinessIdea;
using Waypoint.BusinessIdeas.Application.GetMyBusinessValidations;
using Waypoint.BusinessIdeas.Application.UpdateBusinessIdea;

namespace Waypoint.BusinessIdeas.Api;

public static class BusinessIdeasEndpoints
{
    public static IEndpointRouteBuilder MapBusinessIdeasEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/business-idea").RequireAuthorization().RequireRateLimiting("api").WithTags("BusinessIdeas");

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetMyBusinessIdeaQuery(), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapPut("/", async (UpdateBusinessIdeaCommand command, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(command, ct)));

        group.MapGet("/validations", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetMyBusinessValidationsQuery(), ct)));

        group.MapPost("/validations", async (ISender sender, CancellationToken ct) =>
            Results.Created("/api/v1/business-idea/validations", await sender.Send(new GenerateBusinessValidationCommand(), ct)));

        return app;
    }
}
