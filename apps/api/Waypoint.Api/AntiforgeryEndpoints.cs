using Microsoft.AspNetCore.Antiforgery;

namespace Waypoint.Api;

public static class AntiforgeryEndpoints
{
    /// <summary>
    /// Issues the double-submit CSRF cookie + token pair. Called by the
    /// frontend once on load, before any mutating request — see
    /// docs/05-api-contract.md "Conventions".
    /// </summary>
    public static IEndpointRouteBuilder MapAntiforgeryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/antiforgery/token", (IAntiforgery antiforgery, HttpContext httpContext) =>
            {
                var tokens = antiforgery.GetAndStoreTokens(httpContext);
                return Results.Ok(new { token = tokens.RequestToken });
            })
            .WithTags("Antiforgery");

        return app;
    }
}
