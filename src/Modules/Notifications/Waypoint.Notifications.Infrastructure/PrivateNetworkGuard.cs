using System.Net;
using System.Net.Sockets;

namespace Waypoint.Notifications.Infrastructure;

/// <summary>
/// The authoritative SSRF defense for outbound push delivery. Waypoint.Notifications.Application's
/// EndpointSafety is a first-pass format check applied at subscription-submission time, but DNS
/// can resolve differently between then and actual send time (DNS rebinding) - this is what
/// actually runs at connect time, on every single outbound connection WebPushSender's HttpClient
/// makes, regardless of what a hostname resolved to when first validated. Rejects loopback,
/// link-local (including the 169.254.169.254 cloud metadata address), private RFC1918/IPv6 ULA
/// ranges, and other non-globally-routable addresses before a socket is ever opened.
/// </summary>
public static class PrivateNetworkGuard
{
    public static bool IsPubliclyRoutable(IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
        {
            return false;
        }

        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var b = address.GetAddressBytes();
            if (b[0] == 0) return false; // 0.0.0.0/8
            if (b[0] == 10) return false; // 10.0.0.0/8
            if (b[0] == 100 && b[1] is >= 64 and <= 127) return false; // 100.64.0.0/10 (CGNAT)
            if (b[0] == 127) return false; // 127.0.0.0/8
            if (b[0] == 169 && b[1] == 254) return false; // 169.254.0.0/16 (link-local, incl. cloud metadata)
            if (b[0] == 172 && b[1] is >= 16 and <= 31) return false; // 172.16.0.0/12
            if (b[0] == 192 && b[1] == 168) return false; // 192.168.0.0/16
            if (b[0] >= 224) return false; // multicast + reserved
            return true;
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.IsIPv6Multicast)
            {
                return false;
            }

            var b = address.GetAddressBytes();
            if ((b[0] & 0xfe) == 0xfc) return false; // fc00::/7 - unique local addresses
            return true;
        }

        return false;
    }
}
