using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using WebPush;
using Waypoint.Notifications.Application.Push;
using DomainPushSubscription = Waypoint.Notifications.Domain.PushSubscription;

namespace Waypoint.Notifications.Infrastructure;

/// <summary>
/// Wraps the WebPush NuGet package (RFC 8291 message encryption + VAPID JWT signing - real
/// cryptography this codebase should not hand-roll). Implements the Application-layer IPushSender
/// port (see Waypoint.Notifications.Application/Push/IPushSender.cs) so
/// ReminderDeliveryProcessor's business logic never depends on the WebPush package or on this
/// class directly - only unit-testable seams do.
///
/// The HttpClient used for every outbound send is built with a SocketsHttpHandler.ConnectCallback
/// that enforces PrivateNetworkGuard on every connection - see that class for why this, not the
/// submission-time format check alone (EndpointSafety, Application layer), is the real SSRF
/// control: DNS can resolve differently between subscription-submission time and actual send time.
/// </summary>
public sealed class WebPushSender : IPushSender, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly WebPushClient _client;
    private readonly bool _isConfigured;

    public WebPushSender(VapidOptions options)
    {
        _isConfigured = options.IsConfigured;

        var handler = new SocketsHttpHandler
        {
            ConnectCallback = async (context, cancellationToken) =>
            {
                var entry = await Dns.GetHostEntryAsync(context.DnsEndPoint.Host, cancellationToken);
                var safeAddress = entry.AddressList.FirstOrDefault(PrivateNetworkGuard.IsPubliclyRoutable)
                    ?? throw new InvalidOperationException(
                        $"Refusing to connect to push endpoint host '{context.DnsEndPoint.Host}': " +
                        "no publicly routable address resolved.");

                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
                try
                {
                    await socket.ConnectAsync(safeAddress, context.DnsEndPoint.Port, cancellationToken);
                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch
                {
                    socket.Dispose();
                    throw;
                }
            },
        };

        _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
        _client = new WebPushClient(_httpClient);

        if (_isConfigured)
        {
            _client.SetVapidDetails(options.Subject, options.PublicKey, options.PrivateKey);
        }
    }

    public async Task SendAsync(DomainPushSubscription subscription, PushPayload payload, CancellationToken cancellationToken)
    {
        if (!_isConfigured)
        {
            // Should be unreachable in practice - ScheduledNotificationWorker checks
            // VapidOptions.IsConfigured once at startup and never enters its send path if this is
            // false. This exists as a defensive, clearly-worded failure rather than a confusing
            // WebPushClient internal error, in case this is ever called from anywhere else.
            throw new InvalidOperationException(
                "VAPID keys are not configured - push notifications cannot be sent. " +
                "Set Waypoint:Notifications:Push:VapidPublicKey/VapidPrivateKey/VapidSubject.");
        }

        var webPushSubscription = new WebPush.PushSubscription(
            subscription.Endpoint, subscription.P256dhKey, subscription.AuthKey);

        // Deliberately a small, fixed shape (title/body/url) - not the full delivery-history or
        // account content - see PushPayloadBuilder for the actual privacy decision that produced
        // this payload upstream of here.
        var payloadJson = JsonSerializer.Serialize(new { title = payload.Title, body = payload.Body, url = payload.Url });

        try
        {
            await _client.SendNotificationAsync(webPushSubscription, payloadJson, cancellationToken: cancellationToken);
        }
        catch (WebPushException ex)
        {
            var statusCode = (int)ex.StatusCode;
            var isPermanent = statusCode is 404 or 410;
            throw new PushDeliveryException(isPermanent, statusCode, ex.Message);
        }
    }

    public void Dispose() => _httpClient.Dispose();
}
