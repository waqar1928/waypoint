using System.Net;

namespace Waypoint.Notifications.Application.Push;

/// <summary>
/// First-pass SSRF defense, applied at subscription-submission time: a push endpoint is
/// attacker-controlled input (any authenticated user can submit any string as their "endpoint"),
/// and the server later makes real outbound HTTPS calls to it. A real push endpoint is always a
/// DNS hostname belonging to a browser vendor's push service (fcm.googleapis.com,
/// updates.push.services.mozilla.com, web.push.apple.com, etc.) - an IP-literal host, "localhost",
/// or a non-HTTPS scheme is never legitimate here and is exactly the shape an SSRF attempt takes.
/// This is a first filter only, not the authoritative control: DNS can resolve differently between
/// this check and actual send time (DNS rebinding), so the real, TOCTOU-safe enforcement is the
/// connect-time IP-range guard on the outbound HttpClient used for sending - see
/// Waypoint.Notifications.Infrastructure/PrivateNetworkGuard.cs.
/// </summary>
public static class EndpointSafety
{
    private const int MaxLength = 2048;

    public static bool IsWellFormedHttpsEndpoint(string? endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint) || endpoint.Length > MaxLength)
        {
            return false;
        }

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal))
        {
            return false;
        }

        if (IPAddress.TryParse(uri.Host, out _))
        {
            return false;
        }

        return !string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase);
    }
}
