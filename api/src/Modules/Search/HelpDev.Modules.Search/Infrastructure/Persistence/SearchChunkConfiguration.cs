using HelpDev.Modules.Search.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Search.Infrastructure.Persistence;

public sealed class SearchChunkConfiguration : IEntityTypeConfiguration<SearchChunk>
{
    public void Configure(EntityTypeBuilder<SearchChunk> builder)
    {
        builder.ToTable("search_chunks");
        builder.HasKey(chunk => chunk.Id);
        builder.Property(chunk => chunk.Id).ValueGeneratedNever();

        builder.Property(chunk => chunk.SourceType)
            .IsRequired()
            .HasMaxLength(32)
            .HasColumnName("source_type");

        builder.Property(chunk => chunk.SourceId)
            .IsRequired()
            .HasColumnName("source_id");

        builder.Property(chunk => chunk.ChunkIndex)
            .IsRequired()
            .HasColumnName("chunk_index");

        builder.Property(chunk => chunk.Content)
            .IsRequired()
            .HasColumnType("text")
            .HasColumnName("content");

        builder.Property(chunk => chunk.Title)
            .IsRequired()
            .HasMaxLength(300)
            .HasColumnName("title");

        builder.Property(chunk => chunk.Metadata)
            .HasColumnType("text")
            .HasColumnName("metadata");

        builder.Property(chunk => chunk.CreatedAtUtc)
            .IsRequired()
            .HasColumnName("created_at_utc");

        builder.Property(chunk => chunk.LastEventId)
            .IsRequired()
            .HasColumnName("last_event_id");

        builder.HasIndex(chunk => new { chunk.SourceType, chunk.SourceId, chunk.ChunkIndex })
            .IsUnique()
            .HasDatabaseName("ux_search_chunks_source_index");

        builder.HasIndex(chunk => new { chunk.SourceType, chunk.SourceId })
            .HasDatabaseName("ix_search_chunks_source");
    }
}
