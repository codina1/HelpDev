using HelpDev.Modules.Content.Domain.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

public sealed class ToolMetadataConfiguration : IEntityTypeConfiguration<ToolMetadata>
{
    public void Configure(EntityTypeBuilder<ToolMetadata> builder)
    {
        builder.ToTable("tool_metadata");

        builder.HasKey(tool => tool.Id);
        builder.Property(tool => tool.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(tool => tool.ContentId).HasColumnName("content_id");
        builder.Property(tool => tool.ToolName)
            .HasColumnName("tool_name")
            .HasMaxLength(ToolMetadata.MaxToolNameLength)
            .IsRequired();
        builder.Property(tool => tool.OfficialWebsiteUrl)
            .HasColumnName("official_website_url")
            .HasMaxLength(ToolMetadata.MaxUrlLength)
            .IsRequired();
        builder.Property(tool => tool.GithubUrl)
            .HasColumnName("github_url")
            .HasMaxLength(ToolMetadata.MaxUrlLength);
        builder.Property(tool => tool.LogoMediaId).HasColumnName("logo_media_id");
        builder.Property(tool => tool.CompanyName)
            .HasColumnName("company_name")
            .HasMaxLength(ToolMetadata.MaxCompanyNameLength);
        builder.Property(tool => tool.PricingModel)
            .HasColumnName("pricing_model")
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(tool => tool.ToolCategory)
            .HasColumnName("tool_category")
            .HasMaxLength(ToolMetadata.MaxToolCategoryLength)
            .IsRequired();
        builder.Property(tool => tool.PlatformSupport)
            .HasColumnName("platform_support");
        builder.Property(tool => tool.LicenseType)
            .HasColumnName("license_type")
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(tool => tool.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(tool => tool.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(tool => tool.ContentId)
            .IsUnique()
            .HasDatabaseName("ix_tool_metadata_content_id");

        builder.HasIndex(tool => tool.ToolName)
            .HasDatabaseName("ix_tool_metadata_tool_name");

        builder.HasOne<ContentEntity>()
            .WithMany()
            .HasForeignKey(tool => tool.ContentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_tool_metadata_contents_content_id");

        builder.HasMany(tool => tool.Features)
            .WithOne()
            .HasForeignKey(feature => feature.ToolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(tool => tool.Alternatives)
            .WithOne()
            .HasForeignKey(alternative => alternative.ToolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(tool => tool.Features)
            .HasField("_features")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(tool => tool.Alternatives)
            .HasField("_alternatives")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class ToolFeatureConfiguration : IEntityTypeConfiguration<ToolFeature>
{
    public void Configure(EntityTypeBuilder<ToolFeature> builder)
    {
        builder.ToTable("tool_features");
        builder.HasKey(feature => feature.Id);
        builder.Property(feature => feature.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(feature => feature.ToolId).HasColumnName("tool_id");
        builder.Property(feature => feature.Title)
            .HasColumnName("title")
            .HasMaxLength(ToolFeature.MaxTitleLength)
            .IsRequired();
        builder.Property(feature => feature.Description)
            .HasColumnName("description")
            .HasMaxLength(ToolFeature.MaxDescriptionLength);
        builder.Property(feature => feature.Order).HasColumnName("sort_order");

        builder.HasIndex(feature => new { feature.ToolId, feature.Order })
            .HasDatabaseName("ix_tool_features_tool_id_sort_order");
    }
}

public sealed class ToolAlternativeConfiguration : IEntityTypeConfiguration<ToolAlternative>
{
    public void Configure(EntityTypeBuilder<ToolAlternative> builder)
    {
        builder.ToTable("tool_alternatives");
        builder.HasKey(alternative => alternative.Id);
        builder.Property(alternative => alternative.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(alternative => alternative.ToolId).HasColumnName("tool_id");
        builder.Property(alternative => alternative.AlternativeToolContentId)
            .HasColumnName("alternative_tool_content_id");
        builder.Property(alternative => alternative.Order).HasColumnName("sort_order");

        builder.HasIndex(alternative => new { alternative.ToolId, alternative.Order })
            .HasDatabaseName("ix_tool_alternatives_tool_id_sort_order");

        builder.HasIndex(alternative => new { alternative.ToolId, alternative.AlternativeToolContentId })
            .IsUnique()
            .HasDatabaseName("ix_tool_alternatives_tool_id_alternative_content_id");
    }
}
