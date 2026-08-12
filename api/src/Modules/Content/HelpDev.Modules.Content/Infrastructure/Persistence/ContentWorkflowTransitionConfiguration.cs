using HelpDev.Modules.Content.Domain.Entities;
using HelpDev.Modules.Content.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

public sealed class ContentWorkflowTransitionConfiguration : IEntityTypeConfiguration<ContentWorkflowTransition>
{
    public void Configure(EntityTypeBuilder<ContentWorkflowTransition> builder)
    {
        builder.ToTable("content_workflow_history");

        builder.HasKey(row => row.Id);

        builder.Property(row => row.Id).HasColumnName("id");
        builder.Property(row => row.ContentId).HasColumnName("content_id");
        builder.Property(row => row.FromStatus)
            .HasColumnName("from_status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(row => row.ToStatus)
            .HasColumnName("to_status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(row => row.ActorUserId).HasColumnName("actor_user_id");
        builder.Property(row => row.Comment)
            .HasColumnName("comment")
            .HasMaxLength(ContentWorkflowTransition.MaxCommentLength);
        builder.Property(row => row.CreatedAtUtc).HasColumnName("created_at_utc");

        builder.HasIndex(row => new { row.ContentId, row.CreatedAtUtc })
            .HasDatabaseName("ix_content_workflow_history_content_id_created_at_utc");

        builder.HasIndex(row => new { row.ContentId, row.ToStatus })
            .HasDatabaseName("ix_content_workflow_history_content_id_to_status");
    }
}
