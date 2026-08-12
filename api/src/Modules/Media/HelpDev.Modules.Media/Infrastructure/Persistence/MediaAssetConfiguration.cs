using HelpDev.Modules.Media.Domain.Assets;
using HelpDev.Modules.Media.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Media.Infrastructure.Persistence;

public sealed class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("media_assets");

        builder.HasKey(asset => asset.Id);

        builder.Property(asset => asset.Id).HasColumnName("id");
        builder.Property(asset => asset.OriginalFileName)
            .HasColumnName("original_file_name")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(asset => asset.StorageKey)
            .HasColumnName("storage_key")
            .HasMaxLength(260)
            .IsRequired();
        builder.Property(asset => asset.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(asset => asset.SizeBytes).HasColumnName("size_bytes");
        builder.Property(asset => asset.Width).HasColumnName("width");
        builder.Property(asset => asset.Height).HasColumnName("height");
        builder.Property(asset => asset.PublicUrl)
            .HasColumnName("public_url")
            .HasMaxLength(512)
            .IsRequired();
        builder.Property(asset => asset.AltText)
            .HasColumnName("alt_text")
            .HasMaxLength(200);
        builder.Property(asset => asset.Caption)
            .HasColumnName("caption")
            .HasMaxLength(500);
        builder.Property(asset => asset.UploadedByUserId).HasColumnName("uploaded_by_user_id");
        builder.Property(asset => asset.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(asset => asset.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.Property(asset => asset.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .HasDefaultValue(MediaAssetStatus.Active);

        builder.HasIndex(asset => asset.StorageKey).IsUnique().HasDatabaseName("ix_media_assets_storage_key");
        builder.HasIndex(asset => asset.PublicUrl).IsUnique().HasDatabaseName("ix_media_assets_public_url");
        builder.HasIndex(asset => asset.CreatedAtUtc).HasDatabaseName("ix_media_assets_created_at_utc");
        builder.HasIndex(asset => asset.UploadedByUserId).HasDatabaseName("ix_media_assets_uploaded_by_user_id");
        builder.HasIndex(asset => asset.ContentType).HasDatabaseName("ix_media_assets_content_type");
        builder.HasIndex(asset => asset.Status).HasDatabaseName("ix_media_assets_status");

        builder.Ignore(asset => asset.DomainEvents);
    }
}
