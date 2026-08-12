using HelpDev.Modules.Analytics.Domain.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Analytics.Infrastructure.Persistence;

public sealed class DailyMetricConfiguration : IEntityTypeConfiguration<DailyMetric>
{
    public void Configure(EntityTypeBuilder<DailyMetric> builder)
    {
        builder.ToTable("analytics_daily_metrics");

        builder.HasKey(metric => metric.Id);

        builder.Property(metric => metric.Id)
            .ValueGeneratedNever();

        builder.Property(metric => metric.DateUtc)
            .IsRequired()
            .HasColumnName("date_utc");

        builder.Property(metric => metric.MetricKey)
            .IsRequired()
            .HasMaxLength(Domain.AnalyticsLimits.MaxMetricKeyLength)
            .HasColumnName("metric_key");

        builder.Property(metric => metric.SubjectId)
            .HasColumnName("subject_id");

        builder.Property(metric => metric.SubjectType)
            .HasMaxLength(Domain.AnalyticsLimits.MaxSubjectTypeLength)
            .HasColumnName("subject_type");

        builder.Property(metric => metric.Dimension1Key)
            .IsRequired()
            .HasMaxLength(Domain.AnalyticsLimits.MaxDimensionKeyLength)
            .HasColumnName("dimension1_key");

        builder.Property(metric => metric.Dimension1Value)
            .IsRequired()
            .HasMaxLength(Domain.AnalyticsLimits.MaxDimensionValueLength)
            .HasColumnName("dimension1_value");

        builder.Property(metric => metric.Dimension2Key)
            .IsRequired()
            .HasMaxLength(Domain.AnalyticsLimits.MaxDimensionKeyLength)
            .HasColumnName("dimension2_key");

        builder.Property(metric => metric.Dimension2Value)
            .IsRequired()
            .HasMaxLength(Domain.AnalyticsLimits.MaxDimensionValueLength)
            .HasColumnName("dimension2_value");

        builder.Property(metric => metric.Count)
            .IsRequired()
            .HasColumnName("count");

        builder.Property(metric => metric.SuccessCount)
            .IsRequired()
            .HasColumnName("success_count");

        builder.Property(metric => metric.FailureCount)
            .IsRequired()
            .HasColumnName("failure_count");

        builder.Property(metric => metric.TotalDurationMilliseconds)
            .IsRequired()
            .HasColumnName("total_duration_milliseconds");

        builder.Property(metric => metric.MinDurationMilliseconds)
            .IsRequired()
            .HasColumnName("min_duration_milliseconds");

        builder.Property(metric => metric.MaxDurationMilliseconds)
            .IsRequired()
            .HasColumnName("max_duration_milliseconds");

        builder.Property(metric => metric.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at_utc");

        builder.Property(metric => metric.UpdatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at_utc");

        builder.HasIndex(metric => new
            {
                metric.DateUtc,
                metric.MetricKey,
                metric.SubjectId,
                metric.SubjectType,
                metric.Dimension1Key,
                metric.Dimension1Value,
                metric.Dimension2Key,
                metric.Dimension2Value,
            })
            .IsUnique()
            .AreNullsDistinct(false)
            .HasDatabaseName("ux_analytics_daily_metrics_identity");

        builder.HasIndex(metric => metric.DateUtc)
            .HasDatabaseName("ix_analytics_daily_metrics_date_utc");

        builder.HasIndex(metric => new { metric.MetricKey, metric.DateUtc })
            .HasDatabaseName("ix_analytics_daily_metrics_metric_key_date_utc");
    }
}
