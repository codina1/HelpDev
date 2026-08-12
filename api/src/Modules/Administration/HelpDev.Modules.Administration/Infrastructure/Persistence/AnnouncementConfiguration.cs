using HelpDev.Modules.Administration.Domain.Announcements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Administration.Infrastructure.Persistence;

public sealed class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.ToTable("administration_announcements");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .ValueGeneratedNever();

        builder.Property(item => item.Title)
            .IsRequired()
            .HasMaxLength(Announcement.TitleMaxLength)
            .HasColumnName("title");

        builder.Property(item => item.Body)
            .IsRequired()
            .HasMaxLength(Announcement.BodyMaxLength)
            .HasColumnName("body");

        builder.Property(item => item.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("type");

        builder.Property(item => item.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("status");

        builder.Property(item => item.StartsAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("starts_at_utc");

        builder.Property(item => item.EndsAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("ends_at_utc");

        builder.Property(item => item.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at_utc");

        builder.Property(item => item.UpdatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at_utc");

        builder.Property(item => item.PublishedAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("published_at_utc");

        builder.HasIndex(item => item.Status)
            .HasDatabaseName("ix_administration_announcements_status");

        builder.HasIndex(item => item.Type)
            .HasDatabaseName("ix_administration_announcements_type");

        builder.HasIndex(item => item.StartsAtUtc)
            .HasDatabaseName("ix_administration_announcements_starts_at_utc");

        builder.HasIndex(item => item.EndsAtUtc)
            .HasDatabaseName("ix_administration_announcements_ends_at_utc");

        builder.HasIndex(item => item.UpdatedAtUtc)
            .HasDatabaseName("ix_administration_announcements_updated_at_utc");
    }
}
