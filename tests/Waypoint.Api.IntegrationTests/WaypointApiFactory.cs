using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Xunit;

namespace Waypoint.Api.IntegrationTests;

/// <summary>
/// Spins up a throwaway Postgres container per test run (see
/// docs/07-technical-architecture.md "CI-ready") so these tests exercise
/// real EF Core migrations and the real Identity/Users modules — no mocks,
/// no in-memory provider standing in for Postgres-specific behavior.
/// Requires a Docker daemon; skipped environments should run unit tests only.
/// </summary>
public sealed class WaypointApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("waypoint_test")
        .WithUsername("waypoint")
        .WithPassword("waypoint_test_password")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _postgres.GetConnectionString(),
                ["Waypoint:AutoMigrate"] = "true",
            });
        });
    }

    public async Task InitializeAsync() => await _postgres.StartAsync();

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
