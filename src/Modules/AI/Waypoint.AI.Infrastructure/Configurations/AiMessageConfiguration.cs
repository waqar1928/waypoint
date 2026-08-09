using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.AI.Domain;

namespace Waypoint.AI.Infrastructure.Configurations;

public sealed class AiMessageConfiguration : IEntityTypeConfiguration<AiMessage>
{
    public void Configure(EntityTypeBuilder<AiMessage> builder)
    {
        builder.ToTable("ai_messages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Role).HasConversion<string>().HasMaxLength(10);
        builder.Property(m => m.Content).IsRequired();
        builder.Property(m => m.PromptTemplateVersion).HasMaxLength(20);
        builder.HasIndex(m => new { m.ConversationId, m.CreatedAt });

        // The one place a within-module FK cascades on delete instead of the default RESTRICT —
        // a message has no independent meaning once its conversation is gone (docs/04-database-design.md).
        builder.HasOne<AiConversation>()
            .WithMany()
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.UseXminConcurrencyToken();
    }
}
