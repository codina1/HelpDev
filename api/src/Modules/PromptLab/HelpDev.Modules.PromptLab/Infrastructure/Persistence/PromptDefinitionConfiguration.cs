using HelpDev.Modules.PromptLab.Domain.Categories;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptDefinitionConfiguration : IEntityTypeConfiguration<PromptDefinition>
{
    public void Configure(EntityTypeBuilder<PromptDefinition> builder)
    {
        builder.ToTable("promptlab_prompts");

        builder.HasKey(prompt => prompt.Id);

        builder.Property(prompt => prompt.Id)
            .ValueGeneratedNever();

        builder.Property(prompt => prompt.CategoryId)
            .IsRequired()
            .HasColumnName("category_id");

        builder.HasOne<PromptCategory>()
            .WithMany()
            .HasForeignKey(prompt => prompt.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(prompt => prompt.Name)
            .IsRequired()
            .HasMaxLength(PromptDefinition.NameMaxLength)
            .HasColumnName("name");

        var slugConverter = new ValueConverter<PromptSlug, string>(
            slug => slug.Value,
            value => PromptSlug.FromPersisted(value));

        builder.Property(prompt => prompt.Slug)
            .HasConversion(slugConverter)
            .IsRequired()
            .HasMaxLength(PromptDefinition.SlugMaxLength)
            .HasColumnName("slug");

        builder.HasIndex(prompt => prompt.Slug)
            .IsUnique()
            .HasDatabaseName("ux_promptlab_prompts_slug");

        builder.Property(prompt => prompt.Summary)
            .IsRequired()
            .HasMaxLength(PromptDefinition.SummaryMaxLength)
            .HasColumnName("summary");

        builder.Property(prompt => prompt.Description)
            .HasMaxLength(PromptDefinition.DescriptionMaxLength)
            .HasColumnName("description");

        builder.Property(prompt => prompt.Purpose)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(40)
            .HasColumnName("purpose");

        builder.Property(prompt => prompt.Visibility)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(40)
            .HasColumnName("visibility");

        builder.Property(prompt => prompt.IsEnabled)
            .IsRequired()
            .HasColumnName("is_enabled");

        builder.Property(prompt => prompt.IsPublished)
            .IsRequired()
            .HasColumnName("is_published");

        builder.Property(prompt => prompt.RequiresAuthentication)
            .IsRequired()
            .HasColumnName("requires_authentication");

        builder.Property(prompt => prompt.AllowHistory)
            .IsRequired()
            .HasColumnName("allow_history");

        builder.Property(prompt => prompt.DisplayOrder)
            .IsRequired()
            .HasColumnName("display_order");

        builder.Property(prompt => prompt.LatestVersionNumber)
            .IsRequired()
            .HasColumnName("latest_version_number");

        builder.Property(prompt => prompt.PublishedVersionNumber)
            .HasColumnName("published_version_number");

        builder.Property(prompt => prompt.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at_utc");

        builder.Property(prompt => prompt.UpdatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at_utc");

        builder.Property(prompt => prompt.PublishedAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("published_at_utc");

        builder.HasMany(prompt => prompt.Versions)
            .WithOne()
            .HasForeignKey(version => version.PromptDefinitionId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.Navigation(prompt => prompt.Versions)
            .HasField("_versions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(prompt => prompt.CategoryId)
            .HasDatabaseName("ix_promptlab_prompts_category_id");

        builder.HasIndex(prompt => prompt.Purpose)
            .HasDatabaseName("ix_promptlab_prompts_purpose");

        builder.HasIndex(prompt => prompt.Visibility)
            .HasDatabaseName("ix_promptlab_prompts_visibility");

        builder.HasIndex(prompt => prompt.IsEnabled)
            .HasDatabaseName("ix_promptlab_prompts_is_enabled");

        builder.HasIndex(prompt => prompt.IsPublished)
            .HasDatabaseName("ix_promptlab_prompts_is_published");

        builder.HasIndex(prompt => prompt.DisplayOrder)
            .HasDatabaseName("ix_promptlab_prompts_display_order");

        builder.HasIndex(prompt => prompt.PublishedVersionNumber)
            .HasDatabaseName("ix_promptlab_prompts_published_version_number");
    }
}
