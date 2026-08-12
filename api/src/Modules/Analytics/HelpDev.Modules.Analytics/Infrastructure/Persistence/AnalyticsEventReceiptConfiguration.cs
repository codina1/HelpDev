using HelpDev.Modules.Analytics.Application.Persistence;
using HelpDev.Modules.Analytics.Domain.Events;
using HelpDev.Modules.Analytics.Domain.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Analytics.Infrastructure.Persistence;

public sealed class AnalyticsEventReceiptConfiguration : IEntityTypeConfiguration<AnalyticsEventReceipt>
{
    public void Configure(EntityTypeBuilder<AnalyticsEventReceipt> builder)
    {
        builder.ToTable("analytics_event_receipts");

        builder.HasKey(receipt => receipt.EventId);

        builder.Property(receipt => receipt.EventId)
            .ValueGeneratedNever();

        builder.Property(receipt => receipt.EventType)
            .IsRequired()
            .HasMaxLength(Domain.AnalyticsLimits.MaxEventTypeLength)
            .HasColumnName("event_type");

        builder.Property(receipt => receipt.OccurredAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("occurred_at_utc");

        builder.Property(receipt => receipt.ProcessedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("processed_at_utc");

        builder.Property(receipt => receipt.ProcessingStatus)
            .IsRequired()
            .HasMaxLength(Domain.AnalyticsLimits.MaxProcessingStatusLength)
            .HasColumnName("processing_status");

        builder.Property(receipt => receipt.ErrorCode)
            .HasMaxLength(Domain.AnalyticsLimits.MaxErrorCodeLength)
            .HasColumnName("error_code");

        builder.Property(receipt => receipt.MetricDateUtc)
            .IsRequired()
            .HasColumnName("metric_date_utc");

        builder.Property(receipt => receipt.SchemaVersion)
            .IsRequired()
            .HasColumnName("schema_version");

        builder.HasIndex(receipt => receipt.ProcessedAtUtc)
            .HasDatabaseName("ix_analytics_event_receipts_processed_at_utc");

        builder.HasIndex(receipt => receipt.MetricDateUtc)
            .HasDatabaseName("ix_analytics_event_receipts_metric_date_utc");

        builder.HasIndex(receipt => receipt.EventType)
            .HasDatabaseName("ix_analytics_event_receipts_event_type");
    }
}
