using HelpDev.Modules.PromptLab.Domain;
using HelpDev.Modules.PromptLab.Domain.AiModels;
using HelpDev.Modules.PromptLab.Domain.Categories;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptConfiguration : IEntityTypeConfiguration<Prompt>
{
    public void Configure(EntityTypeBuilder<Prompt> builder)
    {
        builder.ToTable("promptlab_library_prompts");

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

        builder.Property(prompt => prompt.AiModelId)
            .IsRequired()
            .HasColumnName("ai_model_id");

        builder.HasOne<AiModel>()
            .WithMany()
            .HasForeignKey(prompt => prompt.AiModelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(prompt => prompt.Title)
            .IsRequired()
            .HasMaxLength(Prompt.TitleMaxLength)
            .HasColumnName("title");

        var slugConverter = new ValueConverter<PromptSlug, string>(
            slug => slug.Value,
            value => PromptSlug.FromPersisted(value));

        builder.Property(prompt => prompt.Slug)
            .HasConversion(slugConverter)
            .IsRequired()
            .HasMaxLength(Prompt.SlugMaxLength)
            .HasColumnName("slug");

        builder.HasIndex(prompt => prompt.Slug)
            .IsUnique()
            .HasDatabaseName("ux_promptlab_library_prompts_slug");

        builder.Property(prompt => prompt.Description)
            .HasMaxLength(Prompt.DescriptionMaxLength)
            .HasColumnName("description");

        builder.Property(prompt => prompt.Content)
            .IsRequired()
            .HasMaxLength(PromptLabLimits.MaxPromptContentLength)
            .HasColumnName("content");

        builder.Property(prompt => prompt.CoverImage)
            .HasMaxLength(PromptLabLimits.MaxPromptCoverImageLength)
            .HasColumnName("cover_image");

        builder.Property(prompt => prompt.MediaType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(40)
            .HasColumnName("media_type");

        builder.Property(prompt => prompt.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(40)
            .HasDefaultValue(PromptStatus.Draft)
            .HasColumnName("status");

        builder.Property(prompt => prompt.AuthorId)
            .IsRequired()
            .HasColumnName("author_id");

        builder.Property(prompt => prompt.Views)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("views");

        builder.Property(prompt => prompt.CopyCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("copy_count");

        builder.Property(prompt => prompt.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at");

        builder.Property(prompt => prompt.UpdatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at");

        builder.Property(prompt => prompt.PublishedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("published_at");

        builder.HasIndex(prompt => prompt.Status)
            .HasDatabaseName("ix_promptlab_library_prompts_status");

        builder.HasIndex(prompt => prompt.CategoryId)
            .HasDatabaseName("ix_promptlab_library_prompts_category_id");

        builder.HasIndex(prompt => prompt.AiModelId)
            .HasDatabaseName("ix_promptlab_library_prompts_ai_model_id");

        builder.Ignore(prompt => prompt.IsPublic);
        builder.Ignore(prompt => prompt.DomainEvents);
        builder.Ignore(prompt => prompt.HasDomainEvents);
    }
}
