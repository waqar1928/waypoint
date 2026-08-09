using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Waypoint.Audit.Application.GetAuditLog;

namespace Waypoint.Audit.Api;

public static class AuditEndpoints
{
    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/audit-log")
            .RequireAuthorization("Admin")
            .RequireRateLimiting("api")
            .WithTags("Admin");

        group.MapGet("/", async (int? take, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetAuditLogQuery(take ?? 200), ct)));

        return app;
    }
}
