using System.Text.Json;
using HelpDev.Modules.Content.Domain.Entities;
using HelpDev.Modules.Content.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

public sealed class ContentRevisionConfiguration : IEntityTypeConfiguration<ContentRevision>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<ContentRevision> builder)
    {
        builder.ToTable("content_revisions");

        builder.HasKey(revision => revision.Id);

        builder.Property(revision => revision.Id).HasColumnName("id");
        builder.Property(revision => revision.ContentId).HasColumnName("content_id");
        builder.Property(revision => revision.VersionNumber).HasColumnName("version_number");
        builder.Property(revision => revision.ChangeReason)
            .HasColumnName("change_reason")
            .HasMaxLength(ContentRevision.MaxChangeReasonLength);
        builder.Property(revision => revision.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(revision => revision.CreatedAtUtc).HasColumnName("created_at_utc");

        var snapshotConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<ContentRevisionSnapshot, string>(
            snapshot => SerializeSnapshot(snapshot),
            json => DeserializeSnapshot(json));

        var snapshotComparer = new ValueComparer<ContentRevisionSnapshot>(
            (left, right) => left == right,
            value => value.GetHashCode(),
            value => value);

        builder.Property(revision => revision.Snapshot)
            .HasColumnName("snapshot_json")
            .HasColumnType("jsonb")
            .HasConversion(snapshotConverter)
            .Metadata.SetValueComparer(snapshotComparer);

        builder.HasIndex(revision => new { revision.ContentId, revision.VersionNumber })
            .IsUnique()
            .HasDatabaseName("ix_content_revisions_content_id_version_number");

        builder.HasIndex(revision => new { revision.ContentId, revision.CreatedAtUtc })
            .HasDatabaseName("ix_content_revisions_content_id_created_at_utc");
    }

    private static string SerializeSnapshot(ContentRevisionSnapshot snapshot)
    {
        var payload = new SnapshotPayload
        {
            Title = snapshot.Title,
            Slug = snapshot.Slug,
            Body = snapshot.Body,
            Excerpt = snapshot.Excerpt,
            CoverImage = snapshot.CoverImage,
            ContentType = snapshot.ContentType,
            SeoMetadata = new SeoPayload
            {
                SeoTitle = snapshot.SeoMetadata.SeoTitle,
                SeoDescription = snapshot.SeoMetadata.SeoDescription,
                CanonicalUrl = snapshot.SeoMetadata.CanonicalUrl,
                OgImage = snapshot.SeoMetadata.OgImage,
                FocusKeyword = snapshot.SeoMetadata.FocusKeyword,
            },
        };

        return JsonSerializer.Serialize(payload, SerializerOptions);
    }

    private static ContentRevisionSnapshot DeserializeSnapshot(string json)
    {
        var payload = JsonSerializer.Deserialize<SnapshotPayload>(json, SerializerOptions)
            ?? throw new InvalidOperationException("Invalid revision snapshot JSON.");

        var seo = ContentRevisionSeoSnapshot.Create(
            payload.SeoMetadata?.SeoTitle,
            payload.SeoMetadata?.SeoDescription,
            payload.SeoMetadata?.CanonicalUrl,
            payload.SeoMetadata?.OgImage,
            payload.SeoMetadata?.FocusKeyword);

        return ContentRevisionSnapshot.Create(
            payload.Title,
            payload.Slug,
            payload.Body,
            payload.Excerpt,
            payload.CoverImage,
            payload.ContentType,
            seo);
    }

    private sealed class SnapshotPayload
    {
        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string Excerpt { get; set; } = string.Empty;

        public string? CoverImage { get; set; }

        public string ContentType { get; set; } = string.Empty;

        public SeoPayload? SeoMetadata { get; set; }
    }

    private sealed class SeoPayload
    {
        public string? SeoTitle { get; set; }

        public string? SeoDescription { get; set; }

        public string? CanonicalUrl { get; set; }

        public string? OgImage { get; set; }

        public string? FocusKeyword { get; set; }
    }
}
