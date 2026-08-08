using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Actions.Application;
using Waypoint.Common;
using Waypoint.Common.Infrastructure;

namespace Waypoint.Actions.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddActionsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres configuration.");

        services.AddDbContext<ActionsDbContext>(options =>
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(new AuditableEntitySaveChangesInterceptor()));

        services.AddScoped<IActionsRepository, ActionsRepository>();
        services.AddScoped<IStartupMigrator, ActionsStartupMigrator>();

        return services;
    }
}
