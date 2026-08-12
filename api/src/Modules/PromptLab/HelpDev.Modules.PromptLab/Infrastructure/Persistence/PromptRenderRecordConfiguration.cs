using HelpDev.Modules.PromptLab.Domain;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.Modules.PromptLab.Domain.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptRenderRecordConfiguration : IEntityTypeConfiguration<PromptRenderRecord>
{
    public void Configure(EntityTypeBuilder<PromptRenderRecord> builder)
    {
        builder.ToTable("promptlab_render_records");

        builder.HasKey(record => record.Id);

        builder.Property(record => record.Id)
            .ValueGeneratedNever();

        builder.Property(record => record.PromptDefinitionId)
            .IsRequired()
            .HasColumnName("prompt_definition_id");

        builder.HasOne<PromptDefinition>()
            .WithMany()
            .HasForeignKey(record => record.PromptDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(record => record.PromptVersionId)
            .IsRequired()
            .HasColumnName("prompt_version_id");

        builder.HasOne<PromptVersion>()
            .WithMany()
            .HasForeignKey(record => record.PromptVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(record => record.VersionNumber)
            .IsRequired()
            .HasColumnName("version_number");

        builder.Property(record => record.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(record => record.Succeeded)
            .IsRequired()
            .HasColumnName("succeeded");

        builder.Property(record => record.DurationMilliseconds)
            .IsRequired()
            .HasColumnName("duration_milliseconds");

        builder.Property(record => record.InputPreview)
            .HasMaxLength(PromptLabLimits.MaxHistoryInputPreview)
            .HasColumnName("input_preview");

        builder.Property(record => record.RenderedPreview)
            .HasMaxLength(PromptLabLimits.MaxHistoryOutputPreview)
            .HasColumnName("rendered_preview");

        builder.Property(record => record.ErrorCode)
            .HasMaxLength(100)
            .HasColumnName("error_code");

        builder.Property(record => record.RenderedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("rendered_at_utc");

        builder.HasIndex(record => record.UserId)
            .HasDatabaseName("ix_promptlab_render_records_user_id");

        builder.HasIndex(record => record.PromptDefinitionId)
            .HasDatabaseName("ix_promptlab_render_records_prompt_definition_id");

        builder.HasIndex(record => record.PromptVersionId)
            .HasDatabaseName("ix_promptlab_render_records_prompt_version_id");

        builder.HasIndex(record => record.RenderedAtUtc)
            .HasDatabaseName("ix_promptlab_render_records_rendered_at_utc");

        builder.HasIndex(record => record.Succeeded)
            .HasDatabaseName("ix_promptlab_render_records_succeeded");
    }
}
