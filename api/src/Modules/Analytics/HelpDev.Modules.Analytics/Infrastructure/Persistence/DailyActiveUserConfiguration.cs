using HelpDev.Modules.Analytics.Domain.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Analytics.Infrastructure.Persistence;

public sealed class DailyActiveUserConfiguration : IEntityTypeConfiguration<DailyActiveUser>
{
    public void Configure(EntityTypeBuilder<DailyActiveUser> builder)
    {
        builder.ToTable("analytics_daily_active_users");

        builder.HasKey(marker => new { marker.DateUtc, marker.UserId });

        builder.Property(marker => marker.DateUtc)
            .HasColumnName("date_utc");

        builder.Property(marker => marker.UserId)
            .HasColumnName("user_id");

        builder.Property(marker => marker.FirstSeenAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("first_seen_at_utc");

        builder.HasIndex(marker => marker.DateUtc)
            .HasDatabaseName("ix_analytics_daily_active_users_date_utc");

        builder.HasIndex(marker => marker.UserId)
            .HasDatabaseName("ix_analytics_daily_active_users_user_id");
    }
}
