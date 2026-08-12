using HelpDev.Modules.Search.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;

namespace HelpDev.Modules.Search.Infrastructure.Persistence;

public sealed class SearchVectorConfiguration : IEntityTypeConfiguration<SearchVector>
{
    public const int DefaultDimensions = 384;

    public void Configure(EntityTypeBuilder<SearchVector> builder)
    {
        builder.ToTable("search_vectors");
        builder.HasKey(vector => vector.Id);
        builder.Property(vector => vector.Id).ValueGeneratedNever();

        builder.Property(vector => vector.ChunkId)
            .IsRequired()
            .HasColumnName("chunk_id");

        builder.HasIndex(vector => vector.ChunkId)
            .IsUnique()
            .HasDatabaseName("ux_search_vectors_chunk");

        builder.Property(vector => vector.Embedding)
            .HasColumnName("embedding")
            .HasColumnType($"vector({DefaultDimensions})")
            .IsRequired()
            .HasConversion(
                value => new Vector(value),
                value => value.ToArray());

        builder.Property(vector => vector.Dimensions)
            .IsRequired()
            .HasColumnName("dimensions");

        builder.Property(vector => vector.Model)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("model");

        builder.Property(vector => vector.CreatedAtUtc)
            .IsRequired()
            .HasColumnName("created_at_utc");
    }
}
