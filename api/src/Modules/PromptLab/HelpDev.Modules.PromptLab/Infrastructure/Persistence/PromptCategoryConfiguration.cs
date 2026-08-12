using HelpDev.Modules.PromptLab.Domain.Categories;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptCategoryConfiguration : IEntityTypeConfiguration<PromptCategory>
{
    public void Configure(EntityTypeBuilder<PromptCategory> builder)
    {
        builder.ToTable("promptlab_categories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id)
            .ValueGeneratedNever();

        builder.Property(category => category.Name)
            .IsRequired()
            .HasMaxLength(PromptCategory.NameMaxLength)
            .HasColumnName("name");

        var slugConverter = new ValueConverter<PromptSlug, string>(
            slug => slug.Value,
            value => PromptSlug.FromPersisted(value));

        builder.Property(category => category.Slug)
            .HasConversion(slugConverter)
            .IsRequired()
            .HasMaxLength(PromptCategory.SlugMaxLength)
            .HasColumnName("slug");

        builder.HasIndex(category => category.Slug)
            .IsUnique()
            .HasDatabaseName("ux_promptlab_categories_slug");

        builder.Property(category => category.Description)
            .HasMaxLength(PromptCategory.DescriptionMaxLength)
            .HasColumnName("description");

        builder.Property(category => category.Icon)
            .HasMaxLength(PromptCategory.IconMaxLength)
            .HasColumnName("icon");

        builder.Property(category => category.DisplayOrder)
            .IsRequired()
            .HasColumnName("display_order");

        builder.Property(category => category.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(category => category.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at_utc");

        builder.Property(category => category.UpdatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at_utc");

        builder.HasIndex(category => category.IsActive)
            .HasDatabaseName("ix_promptlab_categories_is_active");

        builder.HasIndex(category => category.DisplayOrder)
            .HasDatabaseName("ix_promptlab_categories_display_order");
    }
}
