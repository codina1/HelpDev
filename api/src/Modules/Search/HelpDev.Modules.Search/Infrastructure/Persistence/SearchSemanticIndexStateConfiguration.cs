using HelpDev.Modules.Search.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Search.Infrastructure.Persistence;

public sealed class SearchSemanticIndexStateConfiguration : IEntityTypeConfiguration<SearchSemanticIndexState>
{
    public void Configure(EntityTypeBuilder<SearchSemanticIndexState> builder)
    {
        builder.ToTable("search_semantic_index_states");
        builder.HasKey(state => state.Id);
        builder.Property(state => state.Id).ValueGeneratedNever();

        builder.Property(state => state.SourceType)
            .IsRequired()
            .HasMaxLength(32)
            .HasColumnName("source_type");

        builder.Property(state => state.SourceId)
            .IsRequired()
            .HasColumnName("source_id");

        builder.HasIndex(state => new { state.SourceType, state.SourceId })
            .IsUnique()
            .HasDatabaseName("ux_search_semantic_index_states_source");

        builder.Property(state => state.Status)
            .IsRequired()
            .HasMaxLength(32)
            .HasColumnName("status");

        builder.Property(state => state.ChunkCount)
            .IsRequired()
            .HasColumnName("chunk_count");

        builder.Property(state => state.LastEventId)
            .IsRequired()
            .HasColumnName("last_event_id");

        builder.Property(state => state.LastIndexedAtUtc)
            .HasColumnName("last_indexed_at_utc");

        builder.Property(state => state.FailureCode)
            .HasMaxLength(64)
            .HasColumnName("failure_code");

        builder.Property(state => state.UpdatedAtUtc)
            .IsRequired()
            .HasColumnName("updated_at_utc");

        builder.HasIndex(state => state.Status)
            .HasDatabaseName("ix_search_semantic_index_states_status");
    }
}
