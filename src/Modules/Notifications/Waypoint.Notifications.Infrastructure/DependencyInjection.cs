using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Common;
using Waypoint.Notifications.Application;
using Waypoint.Notifications.Application.Push;

namespace Waypoint.Notifications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres configuration.");

        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());
        services.AddScoped<INotificationSink, NotificationSink>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IPushSubscriptionRepository, PushSubscriptionRepository>();
        services.AddScoped<IDeliveryHistoryRepository, DeliveryHistoryRepository>();
        services.AddScoped<ReminderDeliveryProcessor>();
        services.AddScoped<IStartupMigrator, NotificationsStartupMigrator>();

        // Fail-fast validation of this config (outside Development) happens in Program.cs,
        // mirroring the exact Waypoint:DataProtection:KeysDirectory precedent - this just binds
        // whatever is present. In Development, running with VAPID unconfigured is allowed;
        // ScheduledNotificationWorker checks VapidOptions.IsConfigured once at startup and idles
        // rather than attempting sends if it's false, so local dev never requires a real keypair
        // just to run the app.
        var vapidOptions = VapidOptions.FromConfiguration(configuration);
        services.AddSingleton(vapidOptions);
        services.AddSingleton<IPushSender>(_ => new WebPushSender(vapidOptions));

        var pushEnabled = configuration.GetValue("Waypoint:Notifications:Push:Enabled", true);
        if (pushEnabled)
        {
            services.AddHostedService<ScheduledNotificationWorker>();
        }

        return services;
    }
}
