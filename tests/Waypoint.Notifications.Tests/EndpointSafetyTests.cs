using FluentAssertions;
using Waypoint.Notifications.Application.Push;

namespace Waypoint.Notifications.Tests;

public class EndpointSafetyTests
{
    [Theory]
    [InlineData("https://fcm.googleapis.com/fcm/send/abc123")]
    [InlineData("https://updates.push.services.mozilla.com/wpush/v2/abc123")]
    [InlineData("https://web.push.apple.com/QAB...")]
    public void Accepts_real_push_service_style_endpoints(string endpoint)
    {
        EndpointSafety.IsWellFormedHttpsEndpoint(endpoint).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not a url")]
    [InlineData("http://fcm.googleapis.com/fcm/send/abc")] // not HTTPS
    [InlineData("https://127.0.0.1/fcm/send/abc")] // IP-literal - the SSRF shape
    [InlineData("https://10.0.0.5/internal")] // private-range IP literal
    [InlineData("https://169.254.169.254/latest/meta-data")] // cloud metadata address
    [InlineData("https://localhost/fcm/send/abc")]
    [InlineData("https://[::1]/fcm/send/abc")] // IPv6 loopback literal
    [InlineData("ftp://fcm.googleapis.com/fcm/send/abc")]
    public void Rejects_anything_that_is_not_a_well_formed_HTTPS_hostname_endpoint(string? endpoint)
    {
        EndpointSafety.IsWellFormedHttpsEndpoint(endpoint).Should().BeFalse();
    }

    [Fact]
    public void Rejects_an_endpoint_that_is_too_long()
    {
        var tooLong = "https://fcm.googleapis.com/" + new string('a', 3000);

        EndpointSafety.IsWellFormedHttpsEndpoint(tooLong).Should().BeFalse();
    }
}
