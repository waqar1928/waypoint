using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Common;
using Waypoint.Notifications.Application;

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
        services.AddScoped<IStartupMigrator, NotificationsStartupMigrator>();

        return services;
    }
}
