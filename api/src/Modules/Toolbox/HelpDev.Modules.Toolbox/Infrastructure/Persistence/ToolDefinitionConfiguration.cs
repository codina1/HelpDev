using HelpDev.Modules.Toolbox.Domain.Categories;
using HelpDev.Modules.Toolbox.Domain.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HelpDev.Modules.Toolbox.Infrastructure.Persistence;

public sealed class ToolDefinitionConfiguration : IEntityTypeConfiguration<ToolDefinition>
{
    public void Configure(EntityTypeBuilder<ToolDefinition> builder)
    {
        builder.ToTable("toolbox_tools");

        builder.HasKey(tool => tool.Id);

        builder.Property(tool => tool.Id)
            .ValueGeneratedNever();

        builder.Property(tool => tool.CategoryId)
            .IsRequired()
            .HasColumnName("category_id");

        builder.HasOne<ToolCategory>()
            .WithMany()
            .HasForeignKey(tool => tool.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(tool => tool.Name)
            .IsRequired()
            .HasMaxLength(ToolDefinition.NameMaxLength)
            .HasColumnName("name");

        var slugConverter = new ValueConverter<ToolSlug, string>(
            slug => slug.Value,
            value => ToolSlug.FromPersisted(value));

        builder.Property(tool => tool.Slug)
            .HasConversion(slugConverter)
            .IsRequired()
            .HasMaxLength(ToolDefinition.SlugMaxLength)
            .HasColumnName("slug");

        builder.HasIndex(tool => tool.Slug)
            .IsUnique()
            .HasDatabaseName("ux_toolbox_tools_slug");

        builder.Property(tool => tool.Summary)
            .IsRequired()
            .HasMaxLength(ToolDefinition.SummaryMaxLength)
            .HasColumnName("summary");

        builder.Property(tool => tool.Description)
            .HasMaxLength(ToolDefinition.DescriptionMaxLength)
            .HasColumnName("description");

        builder.Property(tool => tool.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(40)
            .HasColumnName("type");

        builder.Property(tool => tool.InputSchema)
            .IsRequired()
            .HasMaxLength(ToolDefinition.SchemaMaxLength)
            .HasColumnName("input_schema");

        builder.Property(tool => tool.ExampleInput)
            .HasMaxLength(ToolDefinition.SchemaMaxLength)
            .HasColumnName("example_input");

        builder.Property(tool => tool.IsPublished)
            .IsRequired()
            .HasColumnName("is_published");

        builder.Property(tool => tool.IsEnabled)
            .IsRequired()
            .HasColumnName("is_enabled");

        builder.Property(tool => tool.RequiresAuthentication)
            .IsRequired()
            .HasColumnName("requires_authentication");

        builder.Property(tool => tool.AllowHistory)
            .IsRequired()
            .HasColumnName("allow_history");

        builder.Property(tool => tool.DisplayOrder)
            .IsRequired()
            .HasColumnName("display_order");

        builder.Property(tool => tool.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at_utc");

        builder.Property(tool => tool.UpdatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at_utc");

        builder.Property(tool => tool.PublishedAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("published_at_utc");

        builder.HasIndex(tool => tool.CategoryId)
            .HasDatabaseName("ix_toolbox_tools_category_id");

        builder.HasIndex(tool => tool.Type)
            .HasDatabaseName("ix_toolbox_tools_type");

        builder.HasIndex(tool => tool.IsPublished)
            .HasDatabaseName("ix_toolbox_tools_is_published");

        builder.HasIndex(tool => tool.IsEnabled)
            .HasDatabaseName("ix_toolbox_tools_is_enabled");

        builder.HasIndex(tool => tool.DisplayOrder)
            .HasDatabaseName("ix_toolbox_tools_display_order");
    }
}
