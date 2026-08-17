using HelpDev.Modules.PromptLab.Domain.Packs;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptPackItemConfiguration : IEntityTypeConfiguration<PromptPackItem>
{
    public void Configure(EntityTypeBuilder<PromptPackItem> builder)
    {
        builder.ToTable("promptlab_pack_items");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .ValueGeneratedNever();

        builder.Property(item => item.PackId)
            .IsRequired()
            .HasColumnName("pack_id");

        builder.Property(item => item.PromptId)
            .IsRequired()
            .HasColumnName("prompt_id");

        builder.HasOne<Prompt>()
            .WithMany()
            .HasForeignKey(item => item.PromptId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(item => item.Order)
            .IsRequired()
            .HasColumnName("item_order");

        builder.HasIndex(item => new { item.PackId, item.PromptId })
            .IsUnique()
            .HasDatabaseName("ux_promptlab_pack_items_pack_id_prompt_id");

        builder.HasIndex(item => new { item.PackId, item.Order })
            .IsUnique()
            .HasDatabaseName("ux_promptlab_pack_items_pack_id_item_order");

        builder.HasIndex(item => item.PromptId)
            .HasDatabaseName("ix_promptlab_pack_items_prompt_id");
    }
}
