using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Common;
using Waypoint.Common.Infrastructure;
using Waypoint.Experiments.Application;

namespace Waypoint.Experiments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddExperimentsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres configuration.");

        services.AddDbContext<ExperimentsDbContext>(options =>
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(new AuditableEntitySaveChangesInterceptor()));

        services.AddScoped<IExperimentsRepository, ExperimentsRepository>();
        services.AddScoped<IStartupMigrator, ExperimentsStartupMigrator>();

        return services;
    }
}
