namespace Waypoint.Identity.Application;

/// <summary>Bound from configuration ("Waypoint:WebAppBaseUrl") so email links point at the frontend, not the API.</summary>
public sealed class IdentityLinkOptions
{
    public required string WebAppBaseUrl { get; init; }
}
