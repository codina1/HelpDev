using HelpDev.Modules.Analytics.Domain.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Analytics.Infrastructure.Persistence;

public sealed class AnalyticsSubjectSnapshotConfiguration : IEntityTypeConfiguration<AnalyticsSubjectSnapshot>
{
    public void Configure(EntityTypeBuilder<AnalyticsSubjectSnapshot> builder)
    {
        builder.ToTable("analytics_subject_snapshots");

        builder.HasKey(snapshot => snapshot.Id);

        builder.Property(snapshot => snapshot.Id)
            .ValueGeneratedNever();

        builder.Property(snapshot => snapshot.SubjectType)
            .IsRequired()
            .HasMaxLength(Domain.AnalyticsLimits.MaxSubjectTypeLength)
            .HasColumnName("subject_type");

        builder.Property(snapshot => snapshot.SubjectId)
            .HasColumnName("subject_id");

        builder.Property(snapshot => snapshot.DisplayName)
            .IsRequired()
            .HasMaxLength(Domain.AnalyticsLimits.MaxDisplayNameLength)
            .HasColumnName("display_name");

        builder.Property(snapshot => snapshot.Slug)
            .HasMaxLength(Domain.AnalyticsLimits.MaxSlugLength)
            .HasColumnName("slug");

        builder.Property(snapshot => snapshot.UpdatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at_utc");

        builder.HasIndex(snapshot => new { snapshot.SubjectType, snapshot.SubjectId })
            .IsUnique()
            .HasDatabaseName("ux_analytics_subject_snapshots_subject_type_subject_id");

        builder.HasIndex(snapshot => snapshot.SubjectType)
            .HasDatabaseName("ix_analytics_subject_snapshots_subject_type");
    }
}
