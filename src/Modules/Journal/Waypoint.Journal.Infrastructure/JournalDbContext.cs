using Microsoft.EntityFrameworkCore;
using Waypoint.Journal.Domain;

namespace Waypoint.Journal.Infrastructure;

public sealed class JournalDbContext(DbContextOptions<JournalDbContext> options) : DbContext(options)
{
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JournalEntry>(entry =>
        {
            entry.ToTable("journal_entries");
            entry.HasKey(e => e.Id);
            entry.Property(e => e.EntryType).HasConversion<string>().HasMaxLength(20);
            entry.Property(e => e.Body).HasMaxLength(5000).IsRequired();
            entry.HasIndex(e => new { e.UserId, e.CreatedAt });
            entry.Property<uint>("xmin")
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        });
    }
}
