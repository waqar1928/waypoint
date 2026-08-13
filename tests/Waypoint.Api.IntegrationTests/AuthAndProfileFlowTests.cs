using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace Waypoint.Api.IntegrationTests;

public partial class AuthAndProfileFlowTests(WaypointApiFactory factory) : IClassFixture<WaypointApiFactory>
{
    private async Task<(HttpClient Client, string CsrfToken)> CreateClientWithCsrfTokenAsync()
    {
        var client = factory.CreateClient();
        var tokenResponse = await client.GetFromJsonAsync<CsrfTokenResponse>("/api/v1/antiforgery/token");
        return (client, tokenResponse!.Token);
    }

    private static HttpRequestMessage MutatingRequest(HttpMethod method, string path, string csrfToken, object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-CSRF-TOKEN", csrfToken);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }
        return request;
    }

    [GeneratedRegex(@"userId=([0-9a-fA-F-]+)&token=([^""]+)")]
    private static partial Regex VerificationLinkPattern();

    /// <summary>
    /// Pulls the real userId/token pair out of the most recently captured verification email for
    /// the given address — the same values a real user would get from clicking the link in their
    /// inbox, not a faked/shortcut token. See WaypointApiFactory.EmailSender /
    /// CapturingEmailSender for why a real inbox isn't needed to test this for real.
    /// </summary>
    private (string UserId, string Token) ExtractVerificationLink(string toEmail)
    {
        var email = factory.EmailSender.SentEmails.Last(e => e.ToEmail == toEmail);
        var match = VerificationLinkPattern().Match(email.HtmlBody);
        match.Success.Should().BeTrue("the confirmation email should contain a verify-email link");
        return (match.Groups[1].Value, Uri.UnescapeDataString(match.Groups[2].Value));
    }

    [Fact]
    public async Task Register_then_login_then_read_profile_then_logout_end_to_end()
    {
        var (client, csrfToken) = await CreateClientWithCsrfTokenAsync();
        var email = $"alex+{Guid.NewGuid():N}@example.com";

        var registerResponse = await client.SendAsync(MutatingRequest(
            HttpMethod.Post, "/api/v1/auth/register", csrfToken,
            new { displayName = "Alex Rivera", email, password = "GoodPass123" }));
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // A real user must confirm their email before they can log in (see
        // docs/PRODUCTION_READINESS_AUDIT.md's Authentication section) — mirror that here using
        // the real token from the captured confirmation email, not a shortcut around the gate.
        var (userId, token) = ExtractVerificationLink(email);
        var confirmResponse = await client.SendAsync(MutatingRequest(
            HttpMethod.Post, "/api/v1/auth/verify-email", csrfToken, new { userId, token }));
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await client.SendAsync(MutatingRequest(
            HttpMethod.Post, "/api/v1/auth/login", csrfToken,
            new { email, password = "GoodPass123" }));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var profileResponse = await client.GetAsync("/api/v1/me/profile");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await profileResponse.Content.ReadFromJsonAsync<ProfileResponse>();
        profile!.DisplayName.Should().Be("Alex Rivera");

        // Antiforgery tokens are bound to auth state (see AntiforgeryValidationMiddleware /
        // apps/api/Waypoint.Api/Program.cs) — the token fetched before login was issued for the
        // anonymous session and is rejected now that the session is authenticated, exactly like
        // the real frontend client re-fetches a token after login (lib/api-client.ts's
        // invalidateCsrfToken()). Mirror that here rather than reusing the pre-login token.
        var postLoginCsrfToken = (await client.GetFromJsonAsync<CsrfTokenResponse>("/api/v1/antiforgery/token"))!.Token;

        var logoutResponse = await client.SendAsync(
            MutatingRequest(HttpMethod.Post, "/api/v1/auth/logout", postLoginCsrfToken));
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var profileAfterLogout = await client.GetAsync("/api/v1/me/profile");
        profileAfterLogout.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_before_confirming_email_is_rejected_and_resend_then_confirm_then_login_succeeds()
    {
        var (client, csrfToken) = await CreateClientWithCsrfTokenAsync();
        var email = $"alex+{Guid.NewGuid():N}@example.com";

        var registerResponse = await client.SendAsync(MutatingRequest(
            HttpMethod.Post, "/api/v1/auth/register", csrfToken,
            new { displayName = "Alex Rivera", email, password = "GoodPass123" }));
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Right password, unconfirmed account — must be rejected with the specific
        // email-not-confirmed error, not a generic auth failure (see
        // docs/PRODUCTION_READINESS_AUDIT.md's Authentication section).
        var blockedLoginResponse = await client.SendAsync(MutatingRequest(
            HttpMethod.Post, "/api/v1/auth/login", csrfToken, new { email, password = "GoodPass123" }));
        blockedLoginResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var problem = await blockedLoginResponse.Content.ReadFromJsonAsync<ProblemDetailsResponse>();
        problem!.Type.Should().Be("https://drevia.net/errors/email-not-confirmed");

        // Resend must produce a new, real, usable confirmation link — not just a 202 with nothing
        // behind it.
        var resendResponse = await client.SendAsync(
            MutatingRequest(HttpMethod.Post, "/api/v1/auth/resend-verification", csrfToken, new { email }));
        resendResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var (userId, token) = ExtractVerificationLink(email);
        var confirmResponse = await client.SendAsync(MutatingRequest(
            HttpMethod.Post, "/api/v1/auth/verify-email", csrfToken, new { userId, token }));
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await client.SendAsync(MutatingRequest(
            HttpMethod.Post, "/api/v1/auth/login", csrfToken, new { email, password = "GoodPass123" }));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Registering_the_same_email_twice_returns_conflict()
    {
        var (client, csrfToken) = await CreateClientWithCsrfTokenAsync();
        var email = $"alex+{Guid.NewGuid():N}@example.com";
        var payload = new { displayName = "Alex Rivera", email, password = "GoodPass123" };

        var first = await client.SendAsync(MutatingRequest(HttpMethod.Post, "/api/v1/auth/register", csrfToken, payload));
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.SendAsync(MutatingRequest(HttpMethod.Post, "/api/v1/auth/register", csrfToken, payload));
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Mutating_request_without_csrf_token_is_rejected()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email = "x@example.com", password = "whatever" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Health_ready_endpoint_reports_healthy()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private sealed record CsrfTokenResponse(string Token);
    private sealed record ProfileResponse(Guid UserId, string DisplayName);
    private sealed record ProblemDetailsResponse(string Type, string Title, int Status, string? Detail);
}
