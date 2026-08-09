using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Waypoint.Api.IntegrationTests;

public class AuthAndProfileFlowTests(WaypointApiFactory factory) : IClassFixture<WaypointApiFactory>
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

    [Fact]
    public async Task Register_then_login_then_read_profile_then_logout_end_to_end()
    {
        var (client, csrfToken) = await CreateClientWithCsrfTokenAsync();
        var email = $"alex+{Guid.NewGuid():N}@example.com";

        var registerResponse = await client.SendAsync(MutatingRequest(
            HttpMethod.Post, "/api/v1/auth/register", csrfToken,
            new { displayName = "Alex Rivera", email, password = "GoodPass123" }));
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

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
}
