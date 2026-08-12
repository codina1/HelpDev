using HelpDev.Modules.Content.Domain.Articles;
using HelpDev.Modules.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

public sealed class ArticleMetadataConfiguration : IEntityTypeConfiguration<ArticleMetadata>
{
    public void Configure(EntityTypeBuilder<ArticleMetadata> builder)
    {
        builder.ToTable("article_metadata");

        builder.HasKey(metadata => metadata.Id);

        builder.Property(metadata => metadata.Id).HasColumnName("id");
        builder.Property(metadata => metadata.ContentId).HasColumnName("content_id");
        builder.Property(metadata => metadata.CategoryId).HasColumnName("category_id");

        builder.Property(metadata => metadata.DifficultyLevel)
            .HasColumnName("difficulty_level")
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(metadata => metadata.ReadingTimeMinutes).HasColumnName("reading_time_minutes");
        builder.Property(metadata => metadata.IsFeatured).HasColumnName("is_featured");
        builder.Property(metadata => metadata.AllowComments).HasColumnName("allow_comments");
        builder.Property(metadata => metadata.TableOfContentsEnabled)
            .HasColumnName("table_of_contents_enabled");
        builder.Property(metadata => metadata.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(metadata => metadata.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(metadata => metadata.ContentId)
            .IsUnique()
            .HasDatabaseName("ix_article_metadata_content_id");

        builder.HasOne<ContentEntity>()
            .WithMany()
            .HasForeignKey(metadata => metadata.ContentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_article_metadata_contents_content_id");
    }
}
