using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Common;
using Waypoint.Common.Infrastructure;
using Waypoint.Dreams.Application;

namespace Waypoint.Dreams.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDreamsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres configuration.");

        services.AddDbContext<DreamsDbContext>(options =>
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(new AuditableEntitySaveChangesInterceptor()));

        services.AddScoped<IDreamRepository, DreamRepository>();
        services.AddScoped<IStartupMigrator, DreamsStartupMigrator>();
        services.AddSingleton<IDreamDirectionGenerator, HeuristicDreamDirectionGenerator>();

        return services;
    }
}
