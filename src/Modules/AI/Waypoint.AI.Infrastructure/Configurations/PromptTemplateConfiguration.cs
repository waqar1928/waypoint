using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.AI.Domain;

namespace Waypoint.AI.Infrastructure.Configurations;

public sealed class PromptTemplateConfiguration : IEntityTypeConfiguration<PromptTemplate>
{
    public void Configure(EntityTypeBuilder<PromptTemplate> builder)
    {
        builder.ToTable("prompt_templates");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Key).HasMaxLength(100).IsRequired();
        builder.Property(t => t.SystemPrompt).IsRequired();
        builder.Property(t => t.UserPromptFormat).IsRequired();
        builder.HasIndex(t => new { t.Key, t.Version }).IsUnique();
        builder.UseXminConcurrencyToken();
    }
}
