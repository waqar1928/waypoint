namespace Waypoint.Notifications.Infrastructure;

/// <summary>
/// VAPID keypair + contact subject for Web Push, bound from Waypoint:Notifications:Push:* config
/// (env: Waypoint__Notifications__Push__VapidPublicKey / VapidPrivateKey / VapidSubject) -
/// following the exact same "Waypoint:&lt;Section&gt;:&lt;Key&gt;" convention every other piece of
/// config in this codebase uses. The private key must only ever come from an environment variable
/// or secrets store - never source control, never appsettings.*.json. Program.cs fails fast at
/// startup outside Development if this isn't fully configured, mirroring the existing
/// Waypoint:DataProtection:KeysDirectory precedent exactly. In Development, running without VAPID
/// configured is allowed (IsConfigured stays false) - ScheduledNotificationWorker checks this once
/// at startup and simply idles rather than attempting sends, so local dev doesn't require every
/// contributor to generate a real keypair just to run the app.
/// </summary>
public sealed class VapidOptions
{
    public string? Subject { get; init; }
    public string? PublicKey { get; init; }
    public string? PrivateKey { get; init; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Subject) && !string.IsNullOrWhiteSpace(PublicKey) && !string.IsNullOrWhiteSpace(PrivateKey);

    public static VapidOptions FromConfiguration(Microsoft.Extensions.Configuration.IConfiguration configuration) =>
        new()
        {
            Subject = configuration["Waypoint:Notifications:Push:VapidSubject"],
            PublicKey = configuration["Waypoint:Notifications:Push:VapidPublicKey"],
            PrivateKey = configuration["Waypoint:Notifications:Push:VapidPrivateKey"],
        };
}
