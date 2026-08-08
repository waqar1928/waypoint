using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.Dreams.Domain;

namespace Waypoint.Dreams.Infrastructure.Configurations;

public sealed class DreamConfiguration : IEntityTypeConfiguration<Dream>
{
    public void Configure(EntityTypeBuilder<Dream> builder)
    {
        builder.ToTable("dreams");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Title).HasMaxLength(200).IsRequired();
        builder.Property(d => d.Stage).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(d => d.UserId).HasFilter("deleted_at IS NULL");

        builder.HasOne(d => d.Statement)
            .WithOne()
            .HasForeignKey<DreamStatement>(s => s.DreamId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }
}
