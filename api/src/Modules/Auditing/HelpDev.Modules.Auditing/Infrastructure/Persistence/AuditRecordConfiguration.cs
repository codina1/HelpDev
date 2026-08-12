using System.Text.Json;
using HelpDev.Modules.Auditing.Domain.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HelpDev.Modules.Auditing.Infrastructure.Persistence;

public sealed class AuditRecordConfiguration : IEntityTypeConfiguration<AuditRecord>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<AuditRecord> builder)
    {
        builder.ToTable("audit_records");

        builder.HasKey(record => record.Id);

        builder.Property(record => record.Id).HasColumnName("id");
        builder.Property(record => record.OccurredAtUtc).HasColumnName("occurred_at_utc");
        builder.Property(record => record.Category).HasColumnName("category").HasMaxLength(50);
        builder.Property(record => record.Action).HasColumnName("action").HasMaxLength(150);
        builder.Property(record => record.Outcome).HasColumnName("outcome").HasMaxLength(30);
        builder.Property(record => record.ActorUserId).HasColumnName("actor_user_id");
        builder.Property(record => record.ActorType).HasColumnName("actor_type").HasMaxLength(30);
        builder.Property(record => record.SubjectId).HasColumnName("subject_id");
        builder.Property(record => record.SubjectType).HasColumnName("subject_type").HasMaxLength(100);
        builder.Property(record => record.SubjectDisplay).HasColumnName("subject_display").HasMaxLength(200);
        builder.Property(record => record.ReasonCode).HasColumnName("reason_code").HasMaxLength(150);
        builder.Property(record => record.CorrelationId).HasColumnName("correlation_id").HasMaxLength(100);
        builder.Property(record => record.RequestMethod).HasColumnName("request_method").HasMaxLength(10);
        builder.Property(record => record.RequestPathTemplate).HasColumnName("request_path_template").HasMaxLength(300);

        var metadataConverter = new ValueConverter<IReadOnlyDictionary<string, string>?, string?>(
            metadata => metadata == null
                ? null
                : JsonSerializer.Serialize(metadata, SerializerOptions),
            json => string.IsNullOrWhiteSpace(json)
                ? null
                : JsonSerializer.Deserialize<Dictionary<string, string>>(json, SerializerOptions));

        var metadataComparer = new ValueComparer<IReadOnlyDictionary<string, string>?>(
            (left, right) => DictionaryEquals(left, right),
            value => DictionaryHash(value),
            value => value == null
                ? null
                : new Dictionary<string, string>(value, StringComparer.Ordinal));

        builder.Property(record => record.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb")
            .HasConversion(metadataConverter)
            .Metadata.SetValueComparer(metadataComparer);

        builder.Property(record => record.CreatedAtUtc).HasColumnName("created_at_utc");

        builder.HasIndex(record => record.OccurredAtUtc).HasDatabaseName("ix_audit_records_occurred_at_utc");
        builder.HasIndex(record => new { record.Category, record.OccurredAtUtc }).HasDatabaseName("ix_audit_records_category_occurred_at_utc");
        builder.HasIndex(record => new { record.Action, record.OccurredAtUtc }).HasDatabaseName("ix_audit_records_action_occurred_at_utc");
        builder.HasIndex(record => new { record.Outcome, record.OccurredAtUtc }).HasDatabaseName("ix_audit_records_outcome_occurred_at_utc");
        builder.HasIndex(record => new { record.ActorUserId, record.OccurredAtUtc }).HasDatabaseName("ix_audit_records_actor_user_id_occurred_at_utc");
        builder.HasIndex(record => new { record.SubjectType, record.SubjectId, record.OccurredAtUtc }).HasDatabaseName("ix_audit_records_subject_occurred_at_utc");
        builder.HasIndex(record => record.CorrelationId).HasDatabaseName("ix_audit_records_correlation_id");
    }

    private static bool DictionaryEquals(
        IReadOnlyDictionary<string, string>? left,
        IReadOnlyDictionary<string, string>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null || left.Count != right.Count)
        {
            return false;
        }

        foreach (var pair in left)
        {
            if (!right.TryGetValue(pair.Key, out var value)
                || !string.Equals(pair.Value, value, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static int DictionaryHash(IReadOnlyDictionary<string, string>? value)
    {
        if (value is null)
        {
            return 0;
        }

        var hash = new HashCode();
        foreach (var pair in value.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            hash.Add(pair.Key, StringComparer.Ordinal);
            hash.Add(pair.Value, StringComparer.Ordinal);
        }

        return hash.ToHashCode();
    }
}
