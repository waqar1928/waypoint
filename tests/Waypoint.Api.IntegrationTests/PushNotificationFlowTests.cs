using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Waypoint.Notifications.Domain;
using Waypoint.Notifications.Infrastructure;
using Xunit;

namespace Waypoint.Api.IntegrationTests;

/// <summary>
/// End-to-end coverage of the P1 push subscription API against a real Postgres (see
/// WaypointApiFactory) - subscribe, upsert-on-resubscribe, unsubscribe, and the authorization
/// boundary. The Waypoint:Notifications:Push:Vapid* config is deliberately left unset in this test
/// environment (see WaypointApiFactory.ConfigureWebHost, which never sets it), which is itself
/// exercised below: push-public-key must return 404, not a blank/broken key, when push isn't
/// configured - proving the frontend's feature-detection has a real signal to react to.
/// </summary>
public partial class PushNotificationFlowTests(WaypointApiFactory factory) : IClassFixture<WaypointApiFactory>
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

    private (string UserId, string Token) ExtractVerificationLink(string toEmail)
    {
        var email = factory.EmailSender.SentEmails.Last(e => e.ToEmail == toEmail);
        var match = VerificationLinkPattern().Match(email.HtmlBody);
        match.Success.Should().BeTrue();
        return (match.Groups[1].Value, Uri.UnescapeDataString(match.Groups[2].Value));
    }

    /// <summary>Registers, confirms, and logs in a real account - the same real gate every other
    /// integration test in this suite goes through (see AuthAndProfileFlowTests), not a shortcut.
    /// Returns an authenticated client with a fresh post-login CSRF token ready to use.</summary>
    private async Task<(HttpClient Client, string CsrfToken)> CreateAuthenticatedClientAsync()
    {
        var (client, csrfToken) = await CreateClientWithCsrfTokenAsync();
        var email = $"push+{Guid.NewGuid():N}@example.com";

        var registerResponse = await client.SendAsync(MutatingRequest(
            HttpMethod.Post, "/api/v1/auth/register", csrfToken,
            new { displayName = "Push Test User", email, password = "GoodPass123" }));
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var (userId, token) = ExtractVerificationLink(email);
        await client.SendAsync(MutatingRequest(HttpMethod.Post, "/api/v1/auth/verify-email", csrfToken, new { userId, token }));

        await client.SendAsync(MutatingRequest(
            HttpMethod.Post, "/api/v1/auth/login", csrfToken, new { email, password = "GoodPass123" }));

        var postLoginCsrfToken = (await client.GetFromJsonAsync<CsrfTokenResponse>("/api/v1/antiforgery/token"))!.Token;
        return (client, postLoginCsrfToken);
    }

    [Fact]
    public async Task Push_public_key_returns_404_when_VAPID_is_not_configured()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/v1/notifications/push-public-key");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Subscribing_creates_an_active_subscription_the_owner_can_list()
    {
        var (client, csrfToken) = await CreateAuthenticatedClientAsync();
        var endpoint = $"https://fcm.googleapis.com/fcm/send/{Guid.NewGuid():N}";

        var subscribeResponse = await client.SendAsync(MutatingRequest(
            HttpMethod.Post, "/api/v1/notifications/push-subscriptions", csrfToken,
            new { endpoint, keys = new { p256dh = "test-p256dh", auth = "test-auth" }, userAgent = "IntegrationTest/1.0" }));

        subscribeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await subscribeResponse.Content.ReadFromJsonAsync<PushSubscriptionResponse>();
        created!.Status.Should().Be("Active");

        var listResponse = await client.GetAsync("/api/v1/notifications/push-subscriptions");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await listResponse.Content.ReadFromJsonAsync<List<PushSubscriptionResponse>>();
        list!.Should().ContainSingle(s => s.Id == created.Id);
    }

    [Fact]
    public async Task Resubscribing_the_same_endpoint_upserts_rather_than_duplicating()
    {
        var (client, csrfToken) = await CreateAuthenticatedClientAsync();
        var endpoint = $"https://fcm.googleapis.com/fcm/send/{Guid.NewGuid():N}";
        var body = new { endpoint, keys = new { p256dh = "test-p256dh", auth = "test-auth" }, userAgent = "IntegrationTest/1.0" };

        var first = await client.SendAsync(MutatingRequest(HttpMethod.Post, "/api/v1/notifications/push-subscriptions", csrfToken, body));
        var firstCreated = await first.Content.ReadFromJsonAsync<PushSubscriptionResponse>();

        var second = await client.SendAsync(MutatingRequest(HttpMethod.Post, "/api/v1/notifications/push-subscriptions", csrfToken, body));
        var secondCreated = await second.Content.ReadFromJsonAsync<PushSubscriptionResponse>();

        secondCreated!.Id.Should().Be(firstCreated!.Id, "the same endpoint must reuse the same row, never create a duplicate");

        var listResponse = await client.GetAsync("/api/v1/notifications/push-subscriptions");
        var list = await listResponse.Content.ReadFromJsonAsync<List<PushSubscriptionResponse>>();
        list!.Count(s => s.Id == firstCreated.Id).Should().Be(1);
    }

    /// <summary>
    /// Regression test for a real bug found during the P1 production-readiness review: a request
    /// body missing the "keys" object entirely deserializes SubscribePushRequest.Keys as null
    /// (System.Text.Json doesn't enforce non-null constructor parameters at deserialization time),
    /// which used to be dereferenced directly in the endpoint handler and throw a raw
    /// NullReferenceException (surfaced as an opaque 500) instead of the clean 400 every other
    /// malformed request gets. Fixed by falling back to empty strings so the existing NotEmpty()
    /// validator rules catch it properly.
    /// </summary>
    [Fact]
    public async Task A_request_missing_the_keys_object_entirely_returns_a_clean_400_not_a_500()
    {
        var (client, csrfToken) = await CreateAuthenticatedClientAsync();

        var response = await client.SendAsync(MutatingRequest(
            HttpMethod.Post, "/api/v1/notifications/push-subscriptions", csrfToken,
            new { endpoint = $"https://fcm.googleapis.com/fcm/send/{Guid.NewGuid():N}", userAgent = (string?)null }));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Rejects_a_subscription_endpoint_that_is_not_a_well_formed_HTTPS_URL()
    {
        var (client, csrfToken) = await CreateAuthenticatedClientAsync();

        var response = await client.SendAsync(MutatingRequest(
            HttpMethod.Post, "/api/v1/notifications/push-subscriptions", csrfToken,
            new { endpoint = "https://169.254.169.254/metadata", keys = new { p256dh = "x", auth = "y" }, userAgent = (string?)null }));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Unauthenticated_requests_are_rejected()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/notifications/push-subscriptions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Owner_can_unsubscribe_their_own_subscription()
    {
        var (client, csrfToken) = await CreateAuthenticatedClientAsync();
        var endpoint = $"https://fcm.googleapis.com/fcm/send/{Guid.NewGuid():N}";
        var subscribeResponse = await client.SendAsync(MutatingRequest(
            HttpMethod.Post, "/api/v1/notifications/push-subscriptions", csrfToken,
            new { endpoint, keys = new { p256dh = "x", auth = "y" }, userAgent = (string?)null }));
        var created = await subscribeResponse.Content.ReadFromJsonAsync<PushSubscriptionResponse>();

        var freshCsrf = (await client.GetFromJsonAsync<CsrfTokenResponse>("/api/v1/antiforgery/token"))!.Token;
        var deleteResponse = await client.SendAsync(
            MutatingRequest(HttpMethod.Delete, $"/api/v1/notifications/push-subscriptions/{created!.Id}", freshCsrf));

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listResponse = await client.GetAsync("/api/v1/notifications/push-subscriptions");
        var list = await listResponse.Content.ReadFromJsonAsync<List<PushSubscriptionResponse>>();
        list!.Single(s => s.Id == created.Id).Status.Should().Be("Deactivated");
    }

    /// <summary>Same anti-enumeration proof as every other ownership boundary in this codebase
    /// (see AuthAndProfileFlowTests and MarkNotificationReadCommandHandlerTests): a user must
    /// never be able to unsubscribe (or even confirm the existence of) another user's push
    /// subscription. UserId is always derived from the authenticated principal on the server -
    /// there is no client-supplied UserId anywhere in this flow to even attempt to forge.</summary>
    [Fact]
    public async Task Cannot_unsubscribe_someone_elses_subscription()
    {
        var (ownerClient, ownerCsrf) = await CreateAuthenticatedClientAsync();
        var endpoint = $"https://fcm.googleapis.com/fcm/send/{Guid.NewGuid():N}";
        var subscribeResponse = await ownerClient.SendAsync(MutatingRequest(
            HttpMethod.Post, "/api/v1/notifications/push-subscriptions", ownerCsrf,
            new { endpoint, keys = new { p256dh = "x", auth = "y" }, userAgent = (string?)null }));
        var created = await subscribeResponse.Content.ReadFromJsonAsync<PushSubscriptionResponse>();

        var (attackerClient, attackerCsrf) = await CreateAuthenticatedClientAsync();
        var deleteResponse = await attackerClient.SendAsync(
            MutatingRequest(HttpMethod.Delete, $"/api/v1/notifications/push-subscriptions/{created!.Id}", attackerCsrf));

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// The actual, authoritative proof of the P1 deduplication design: the database constraint
    /// itself, not just application code that's supposed to respect it. A second insert for the
    /// same (UserId, ReminderKey) must be rejected by Postgres regardless of what application code
    /// does or doesn't check first - this is what makes duplicate sends impossible even under
    /// concurrent workers, not merely unlikely.
    /// </summary>
    [Fact]
    public async Task Delivery_history_unique_constraint_rejects_a_second_row_for_the_same_user_and_reminder_key()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        var userId = Guid.NewGuid();
        const string reminderKey = "daily-next-move:2026-08-21";

        db.DeliveryHistory.Add(new NotificationDeliveryHistory { UserId = userId, ReminderKey = reminderKey });
        await db.SaveChangesAsync();

        db.DeliveryHistory.Add(new NotificationDeliveryHistory { UserId = userId, ReminderKey = reminderKey });
        var act = async () => await db.SaveChangesAsync();

        var thrown = await act.Should().ThrowAsync<DbUpdateException>();
        var isUniqueViolation = thrown.Which.InnerException is PostgresException { SqlState: "23505" };
        isUniqueViolation.Should().BeTrue("a second row for the same (UserId, ReminderKey) must violate the unique index");
    }

    [Fact]
    public async Task Delivery_history_allows_the_same_user_a_different_reminder_key_on_a_different_day()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        var userId = Guid.NewGuid();

        db.DeliveryHistory.Add(new NotificationDeliveryHistory { UserId = userId, ReminderKey = "daily-next-move:2026-08-21" });
        db.DeliveryHistory.Add(new NotificationDeliveryHistory { UserId = userId, ReminderKey = "daily-next-move:2026-08-22" });

        var act = async () => await db.SaveChangesAsync();

        await act.Should().NotThrowAsync();
    }

    private string GetConnectionString()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        return db.Database.GetConnectionString()!;
    }

    /// <summary>
    /// The claim mechanism itself (mirrors ScheduledNotificationWorker.TryClaimAsync's exact SQL),
    /// exercised with genuinely concurrent connections rather than a single in-process
    /// SaveChanges - this is what production actually does when a worker tick fires (or two
    /// overlapping ticks/instances fire at once). Ten simultaneous attempts to claim the SAME new
    /// logical reminder must yield exactly one winner, proven against a real Postgres, not
    /// inferred from the unique constraint alone.
    /// </summary>
    [Fact]
    public async Task Concurrent_claim_attempts_for_a_brand_new_reminder_let_exactly_one_succeed()
    {
        var connectionString = GetConnectionString();
        var userId = Guid.NewGuid();
        var reminderKey = $"daily-next-move:{Guid.NewGuid():N}";

        async Task<Guid?> TryClaimAsync()
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            var id = Guid.NewGuid();
            await using var command = new NpgsqlCommand(
                """
                INSERT INTO notifications_delivery_history (id, user_id, reminder_key, status, attempted_at, retry_count)
                VALUES (@id, @userId, @reminderKey, 'Attempted', now(), 0)
                ON CONFLICT (user_id, reminder_key) DO NOTHING
                RETURNING id
                """,
                connection);
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("userId", userId);
            command.Parameters.AddWithValue("reminderKey", reminderKey);
            var result = await command.ExecuteScalarAsync();
            return result as Guid?;
        }

        var results = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => TryClaimAsync()));

        results.Count(r => r is not null).Should().Be(1, "exactly one of ten simultaneous claim attempts for the same (user, reminder key) must win");
    }

    /// <summary>
    /// The stale-attempt sweep's claim mechanism (mirrors ScheduledNotificationWorker.
    /// SweepStaleAttemptsAsync's exact SQL) under a genuinely held, overlapping transaction -
    /// unlike a fresh INSERT (handled natively by the unique constraint above regardless of
    /// timing), this is the case that specifically needs SELECT ... FOR UPDATE SKIP LOCKED: an
    /// existing row that a second worker might otherwise see and re-claim while the first is still
    /// actively processing it.
    /// </summary>
    [Fact]
    public async Task Stale_sweep_SKIP_LOCKED_prevents_a_second_transaction_from_claiming_a_row_still_locked_by_the_first()
    {
        var connectionString = GetConnectionString();

        using (var seedScope = factory.Services.CreateScope())
        {
            var seedDb = seedScope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
            seedDb.DeliveryHistory.Add(new NotificationDeliveryHistory
            {
                UserId = Guid.NewGuid(),
                ReminderKey = $"daily-next-move:{Guid.NewGuid():N}",
                Status = DeliveryStatus.Attempted,
                AttemptedAt = DateTimeOffset.UtcNow.AddMinutes(-10),
                RetryCount = 0,
            });
            await seedDb.SaveChangesAsync();
        }

        var rowId = await GetSeededRowIdAsync(connectionString);

        await using var connectionA = new NpgsqlConnection(connectionString);
        await connectionA.OpenAsync();
        await using var transactionA = await connectionA.BeginTransactionAsync();

        // Transaction A locks the row and deliberately does not commit yet - simulating a worker
        // that has claimed this row and is still mid-processing.
        await using (var lockCommand = new NpgsqlCommand(
            "SELECT id FROM notifications_delivery_history WHERE id = @id FOR UPDATE SKIP LOCKED", connectionA, transactionA))
        {
            lockCommand.Parameters.AddWithValue("id", rowId);
            var lockedId = await lockCommand.ExecuteScalarAsync();
            lockedId.Should().Be(rowId, "transaction A must successfully acquire the row lock before B attempts to claim it");
        }

        // While A still holds the lock, B runs the exact claim SQL the stale-attempt sweep uses -
        // it must see zero rows (SKIP LOCKED), not wait for A, not error, and not double-claim.
        var claimedByB = await TryClaimStaleRowAsync(connectionString, rowId);
        claimedByB.Should().BeFalse("the row is still locked by transaction A, so a second concurrent claimer must skip it entirely");

        await transactionA.CommitAsync();

        // Once A releases the lock, the row becomes claimable again - SKIP LOCKED only skips WHILE
        // locked, it doesn't permanently exclude the row.
        var claimedByC = await TryClaimStaleRowAsync(connectionString, rowId);
        claimedByC.Should().BeTrue("once transaction A commits and releases the lock, the row must become claimable again");
    }

    private static async Task<Guid> GetSeededRowIdAsync(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "SELECT id FROM notifications_delivery_history WHERE status = 'Attempted' ORDER BY attempted_at DESC LIMIT 1", connection);
        return (Guid)(await command.ExecuteScalarAsync())!;
    }

    private static async Task<bool> TryClaimStaleRowAsync(string connectionString, Guid rowId)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            UPDATE notifications_delivery_history
            SET retry_count = retry_count + 1
            WHERE id IN (
                SELECT id FROM notifications_delivery_history
                WHERE status = 'Attempted' AND id = @id
                FOR UPDATE SKIP LOCKED
            )
            RETURNING id
            """,
            connection);
        command.Parameters.AddWithValue("id", rowId);
        var result = await command.ExecuteScalarAsync();
        return result is not null;
    }
}

public sealed record PushSubscriptionResponse(Guid Id, string? UserAgent, DateTimeOffset CreatedAt, DateTimeOffset LastSeenAt, string Status);

/// <summary>Duplicated from AuthAndProfileFlowTests' private nested record rather than shared -
/// it's one line, and sharing it would mean introducing a base class or a new file just for a
/// single DTO shape.</summary>
public sealed record CsrfTokenResponse(string Token);
