using HelpDev.Modules.PromptLab.Domain;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptVersionConfiguration : IEntityTypeConfiguration<PromptVersion>
{
    public void Configure(EntityTypeBuilder<PromptVersion> builder)
    {
        builder.ToTable("promptlab_versions");

        builder.HasKey(version => version.Id);

        builder.Property(version => version.Id)
            .ValueGeneratedNever();

        builder.Property(version => version.PromptDefinitionId)
            .IsRequired()
            .HasColumnName("prompt_definition_id");

        builder.Property(version => version.VersionNumber)
            .IsRequired()
            .HasColumnName("version_number");

        builder.Property(version => version.Template)
            .IsRequired()
            .HasMaxLength(PromptLabLimits.MaxTemplateLength)
            .HasColumnName("template");

        builder.Property(version => version.ChangeNotes)
            .HasMaxLength(PromptLabLimits.MaxChangeNotesLength)
            .HasColumnName("change_notes");

        builder.Property(version => version.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(version => version.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at_utc");

        builder.HasMany(version => version.Variables)
            .WithOne()
            .HasForeignKey(variable => variable.PromptVersionId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Navigation(version => version.Variables)
            .HasField("_variables")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(version => new { version.PromptDefinitionId, version.VersionNumber })
            .IsUnique()
            .HasDatabaseName("ux_promptlab_versions_prompt_definition_id_version_number");

        builder.HasIndex(version => version.PromptDefinitionId)
            .HasDatabaseName("ix_promptlab_versions_prompt_definition_id");

        builder.HasIndex(version => version.CreatedAtUtc)
            .HasDatabaseName("ix_promptlab_versions_created_at_utc");
    }
}
