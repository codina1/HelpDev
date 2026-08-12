using HelpDev.Modules.Toolbox.Domain.Categories;
using HelpDev.Modules.Toolbox.Domain.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HelpDev.Modules.Toolbox.Infrastructure.Persistence;

public sealed class ToolCategoryConfiguration : IEntityTypeConfiguration<ToolCategory>
{
    public void Configure(EntityTypeBuilder<ToolCategory> builder)
    {
        builder.ToTable("toolbox_categories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id)
            .ValueGeneratedNever();

        builder.Property(category => category.Name)
            .IsRequired()
            .HasMaxLength(ToolCategory.NameMaxLength)
            .HasColumnName("name");

        var slugConverter = new ValueConverter<ToolSlug, string>(
            slug => slug.Value,
            value => ToolSlug.FromPersisted(value));

        builder.Property(category => category.Slug)
            .HasConversion(slugConverter)
            .IsRequired()
            .HasMaxLength(ToolCategory.SlugMaxLength)
            .HasColumnName("slug");

        builder.HasIndex(category => category.Slug)
            .IsUnique()
            .HasDatabaseName("ux_toolbox_categories_slug");

        builder.Property(category => category.Description)
            .HasMaxLength(ToolCategory.DescriptionMaxLength)
            .HasColumnName("description");

        builder.Property(category => category.Icon)
            .HasMaxLength(ToolCategory.IconMaxLength)
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
            .HasDatabaseName("ix_toolbox_categories_is_active");

        builder.HasIndex(category => category.DisplayOrder)
            .HasDatabaseName("ix_toolbox_categories_display_order");
    }
}
