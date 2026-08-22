using HelpDev.Modules.Content.Domain.Articles;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

public class ContentConfiguration : IEntityTypeConfiguration<ContentEntity>
{
    public void Configure(EntityTypeBuilder<ContentEntity> builder)
    {
        builder.ToTable("contents");

        builder.HasKey(content => content.Id);

        builder.Property(content => content.Id)
            .ValueGeneratedNever();

        builder.Property(content => content.Title)
            .IsRequired()
            .HasMaxLength(300)
            .HasColumnName("title");

        var slugConverter = new ValueConverter<Slug, string>(
            slug => slug.Value,
            value => Slug.FromPersisted(value));

        builder.Property(content => content.Slug)
            .HasConversion(slugConverter)
            .IsRequired()
            .HasMaxLength(300)
            .HasColumnName("slug");

        builder.HasIndex(content => content.Slug)
            .IsUnique();

        builder.Property(content => content.Body)
            .IsRequired()
            .HasColumnType("text")
            .HasColumnName("body");

        builder.Property(content => content.Excerpt)
            .IsRequired()
            .HasMaxLength(ContentEntity.MaxExcerptLength)
            .HasDefaultValue(string.Empty)
            .HasColumnName("excerpt");

        builder.Property(content => content.CoverImage)
            .HasMaxLength(ContentEntity.MaxCoverImageLength)
            .HasColumnName("cover_image");

        builder.Property(content => content.ContentJson)
            .HasColumnType("jsonb")
            .HasColumnName("content_json");

        builder.Property(content => content.ContentHtml)
            .HasColumnType("text")
            .HasColumnName("content_html");

        builder.Property(content => content.ContentFormat)
            .HasMaxLength(ArticleEditorLimits.MaxContentFormatLength)
            .HasColumnName("content_format");

        builder.Property(content => content.EditorVersion)
            .HasMaxLength(ArticleEditorLimits.MaxEditorVersionLength)
            .HasColumnName("editor_version");

        builder.Property(content => content.WordCount)
            .HasColumnName("word_count");

        builder.Property(content => content.ReadingTimeMinutes)
            .HasColumnName("reading_time_minutes");

        builder.Property(content => content.LastAutosavedAtUtc)
            .HasColumnName("last_autosaved_at_utc");

        builder.Property(content => content.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("type");

        builder.Property(content => content.AuthorId)
            .IsRequired()
            .HasColumnName("author_id");

        builder.Property(content => content.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ContentStatus.Draft)
            .HasColumnName("status");

        builder.Property(content => content.Views)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("views");

        builder.Property(content => content.Saves)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("saves");

        builder.Property(content => content.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(content => content.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");

        builder.Property(content => content.PublishedAtUtc)
            .HasColumnName("published_at_utc");

        builder.ComplexProperty(content => content.SeoMetadata, seo =>
        {
            seo.Property(metadata => metadata.SeoTitle)
                .HasMaxLength(SeoMetadata.MaxSeoTitleLength)
                .HasColumnName("seo_title");

            seo.Property(metadata => metadata.SeoDescription)
                .HasMaxLength(SeoMetadata.MaxSeoDescriptionLength)
                .HasColumnName("seo_description");

            seo.Property(metadata => metadata.CanonicalUrl)
                .HasMaxLength(SeoMetadata.MaxCanonicalUrlLength)
                .HasColumnName("canonical_url");

            seo.Property(metadata => metadata.OgImage)
                .HasMaxLength(SeoMetadata.MaxOgImageLength)
                .HasColumnName("og_image");

            seo.Property(metadata => metadata.FocusKeyword)
                .HasMaxLength(SeoMetadata.MaxFocusKeywordLength)
                .HasColumnName("focus_keyword");
        });

        builder.HasIndex(content => content.Type);
        builder.HasIndex(content => content.Status);
        builder.HasIndex(content => content.AuthorId);
        builder.HasIndex(content => content.UpdatedAt);
    }
}