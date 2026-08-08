using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Common;
using Waypoint.Common.Infrastructure;
using Waypoint.Identity.Application;
using Waypoint.Users.Application;

namespace Waypoint.Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres configuration.");

        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(new AuditableEntitySaveChangesInterceptor()));
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IStartupMigrator, UsersStartupMigrator>();
        services.AddScoped<IOnboardingStatusProvider, OnboardingStatusProvider>();

        return services;
    }
}
