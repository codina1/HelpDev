using HelpDev.Modules.Search.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Search.Infrastructure.Persistence;

public sealed class SearchDocumentConfiguration : IEntityTypeConfiguration<SearchDocument>
{
    public void Configure(EntityTypeBuilder<SearchDocument> builder)
    {
        builder.ToTable("search_documents");

        builder.HasKey(document => document.Id);

        builder.Property(document => document.Id)
            .ValueGeneratedNever();

        builder.Property(document => document.SourceType)
            .IsRequired()
            .HasMaxLength(32)
            .HasColumnName("source_type");

        builder.Property(document => document.SourceId)
            .IsRequired()
            .HasColumnName("source_id");

        builder.HasIndex(document => new { document.SourceType, document.SourceId })
            .IsUnique()
            .HasDatabaseName("ux_search_documents_source");

        builder.Property(document => document.Title)
            .IsRequired()
            .HasMaxLength(300)
            .HasColumnName("title");

        builder.Property(document => document.Slug)
            .IsRequired()
            .HasMaxLength(300)
            .HasColumnName("slug");

        builder.Property(document => document.Summary)
            .IsRequired()
            .HasColumnType("text")
            .HasColumnName("summary");

        builder.Property(document => document.Url)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnName("url");

        builder.Property(document => document.IsPublished)
            .IsRequired()
            .HasColumnName("is_published");

        builder.Property(document => document.SourcePublishedAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("source_published_at_utc");

        builder.Property(document => document.SourceUpdatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("source_updated_at_utc");

        builder.Property(document => document.IndexedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("indexed_at_utc");

        builder.Property(document => document.LastEventId)
            .IsRequired()
            .HasColumnName("last_event_id");

        builder.HasIndex(document => document.IsPublished)
            .HasDatabaseName("ix_search_documents_is_published");

        builder.HasIndex(document => new { document.IsPublished, document.SourceType, document.Title })
            .HasDatabaseName("ix_search_documents_published_type_title");
    }
}
