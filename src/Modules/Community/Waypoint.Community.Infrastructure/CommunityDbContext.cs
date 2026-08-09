using Microsoft.EntityFrameworkCore;
using Waypoint.Community.Domain;

namespace Waypoint.Community.Infrastructure;

public sealed class CommunityDbContext(DbContextOptions<CommunityDbContext> options) : DbContext(options)
{
    public DbSet<CommunityPost> Posts => Set<CommunityPost>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<ContentReportRecord> ContentReports => Set<ContentReportRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommunityDbContext).Assembly);
    }
}
