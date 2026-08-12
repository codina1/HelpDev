using HelpDev.Modules.Analytics.Domain.AiUsage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Analytics.Infrastructure.Persistence;

public sealed class AiUsageRecordConfiguration : IEntityTypeConfiguration<AiUsageRecord>
{
    public void Configure(EntityTypeBuilder<AiUsageRecord> builder)
    {
        builder.ToTable("ai_usage_records");

        builder.HasKey(record => record.Id);
        builder.Property(record => record.Id).ValueGeneratedNever();

        builder.Property(record => record.UserId)
            .HasColumnName("user_id");

        builder.Property(record => record.TaskType)
            .IsRequired()
            .HasMaxLength(64)
            .HasColumnName("task_type");

        builder.Property(record => record.Provider)
            .IsRequired()
            .HasMaxLength(64)
            .HasColumnName("provider");

        builder.Property(record => record.Model)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("model");

        builder.Property(record => record.InputTokens)
            .IsRequired()
            .HasColumnName("input_tokens");

        builder.Property(record => record.OutputTokens)
            .IsRequired()
            .HasColumnName("output_tokens");

        builder.Property(record => record.ContentId)
            .HasColumnName("content_id");

        builder.Property(record => record.Success)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("success");

        builder.Property(record => record.DurationMs)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("duration_ms");

        builder.Property(record => record.ErrorCode)
            .HasMaxLength(64)
            .HasColumnName("error_code");

        builder.Property(record => record.CreatedAtUtc)
            .IsRequired()
            .HasColumnName("created_at_utc");

        builder.HasIndex(record => record.CreatedAtUtc)
            .HasDatabaseName("ix_ai_usage_records_created_at");

        builder.HasIndex(record => new { record.UserId, record.CreatedAtUtc })
            .HasDatabaseName("ix_ai_usage_records_user_created");

        builder.HasIndex(record => new { record.TaskType, record.CreatedAtUtc })
            .HasDatabaseName("ix_ai_usage_records_task_created");

        builder.HasIndex(record => new { record.Success, record.CreatedAtUtc })
            .HasDatabaseName("ix_ai_usage_records_success_created");
    }
}
