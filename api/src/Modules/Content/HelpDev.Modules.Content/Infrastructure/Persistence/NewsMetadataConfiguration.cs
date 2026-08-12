using HelpDev.Modules.Content.Domain.News;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

public sealed class NewsMetadataConfiguration : IEntityTypeConfiguration<NewsMetadata>
{
    public void Configure(EntityTypeBuilder<NewsMetadata> builder)
    {
        builder.ToTable("news_metadata");

        builder.HasKey(metadata => metadata.Id);

        builder.Property(metadata => metadata.Id).HasColumnName("id");
        builder.Property(metadata => metadata.ContentId).HasColumnName("content_id");
        builder.Property(metadata => metadata.SourceName)
            .HasColumnName("source_name")
            .HasMaxLength(NewsMetadata.MaxSourceNameLength)
            .IsRequired();
        builder.Property(metadata => metadata.SourceUrl)
            .HasColumnName("source_url")
            .HasMaxLength(NewsMetadata.MaxSourceUrlLength);
        builder.Property(metadata => metadata.NewsDateUtc).HasColumnName("news_date_utc");
        builder.Property(metadata => metadata.Priority)
            .HasColumnName("priority")
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(metadata => metadata.ExternalReference)
            .HasColumnName("external_reference")
            .HasMaxLength(NewsMetadata.MaxExternalReferenceLength);
        builder.Property(metadata => metadata.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(metadata => metadata.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(metadata => metadata.ContentId)
            .IsUnique()
            .HasDatabaseName("ix_news_metadata_content_id");

        builder.HasOne<ContentEntity>()
            .WithMany()
            .HasForeignKey(metadata => metadata.ContentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_news_metadata_contents_content_id");
    }
}
