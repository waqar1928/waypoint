using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Common;
using Waypoint.Common.Infrastructure;
using Waypoint.Goals.Application;

namespace Waypoint.Goals.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddGoalsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres configuration.");

        services.AddDbContext<GoalsDbContext>(options =>
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(new AuditableEntitySaveChangesInterceptor()));

        services.AddScoped<IGoalsRepository, GoalsRepository>();
        services.AddScoped<IStartupMigrator, GoalsStartupMigrator>();
        services.AddSingleton<IPlanDraftGenerator, HeuristicPlanDraftGenerator>();

        return services;
    }
}
