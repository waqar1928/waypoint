using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Waypoint.Journal.Application.CreateJournalEntry;
using Waypoint.Journal.Application.GetMyJournalEntries;

namespace Waypoint.Journal.Api;

public static class JournalEndpoints
{
    public static IEndpointRouteBuilder MapJournalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/journal").RequireAuthorization().RequireRateLimiting("api").WithTags("Journal");

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetMyJournalEntriesQuery(), ct)));

        group.MapPost("/", async (CreateJournalEntryCommand command, ISender sender, CancellationToken ct) =>
            Results.Created("/api/v1/journal", await sender.Send(command, ct)));

        return app;
    }
}
