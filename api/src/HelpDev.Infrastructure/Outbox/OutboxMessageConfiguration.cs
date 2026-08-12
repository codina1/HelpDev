using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Infrastructure.Outbox;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public const int TypeMaxLength = 200;
    public const int ErrorMaxLength = 2000;
    public const int LockIdMaxLength = 64;

    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id)
            .ValueGeneratedNever();

        builder.Property(message => message.OccurredAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("occurred_at_utc");

        builder.Property(message => message.Type)
            .IsRequired()
            .HasMaxLength(TypeMaxLength)
            .HasColumnName("type");

        builder.Property(message => message.Payload)
            .IsRequired()
            .HasColumnType("text")
            .HasColumnName("payload");

        builder.Property(message => message.ProcessedAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("processed_at_utc");

        builder.Property(message => message.AttemptCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("attempt_count");

        builder.Property(message => message.LastAttemptAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("last_attempt_at_utc");

        builder.Property(message => message.Error)
            .HasMaxLength(ErrorMaxLength)
            .HasColumnName("error");

        builder.Property(message => message.LockedUntilUtc)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("locked_until_utc");

        builder.Property(message => message.LockId)
            .HasMaxLength(LockIdMaxLength)
            .HasColumnName("lock_id");

        builder.HasIndex(message => new { message.ProcessedAtUtc, message.OccurredAtUtc })
            .HasDatabaseName("ix_outbox_messages_processed_occurred");

        builder.HasIndex(message => message.LockedUntilUtc)
            .HasDatabaseName("ix_outbox_messages_locked_until");

        builder.HasIndex(message => new { message.ProcessedAtUtc, message.AttemptCount, message.OccurredAtUtc })
            .HasDatabaseName("ix_outbox_messages_pending");
    }
}
