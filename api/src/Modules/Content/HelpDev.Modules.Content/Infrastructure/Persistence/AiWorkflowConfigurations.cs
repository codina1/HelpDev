using HelpDev.Modules.Content.Domain.AiWorkflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

public sealed class ContentIdeaConfiguration : IEntityTypeConfiguration<ContentIdea>
{
    public void Configure(EntityTypeBuilder<ContentIdea> builder)
    {
        builder.ToTable("content_ideas");
        builder.HasKey(idea => idea.Id);
        builder.Property(idea => idea.Id).HasColumnName("id");
        builder.Property(idea => idea.Title)
            .HasColumnName("title")
            .HasMaxLength(ContentIdea.TitleMaxLength)
            .IsRequired();
        builder.Property(idea => idea.Description)
            .HasColumnName("description")
            .HasMaxLength(ContentIdea.DescriptionMaxLength)
            .IsRequired();
        builder.Property(idea => idea.TargetType)
            .HasColumnName("target_type")
            .HasMaxLength(ContentIdea.TargetTypeMaxLength)
            .IsRequired();
        builder.Property(idea => idea.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(idea => idea.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(idea => idea.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(idea => idea.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.HasIndex(idea => idea.CreatedByUserId).HasDatabaseName("ix_content_ideas_created_by");
        builder.HasIndex(idea => idea.Status).HasDatabaseName("ix_content_ideas_status");
        builder.Ignore(idea => idea.DomainEvents);
        builder.Ignore(idea => idea.HasDomainEvents);
    }
}

public sealed class AiContentWorkflowSessionConfiguration : IEntityTypeConfiguration<AiContentWorkflowSession>
{
    public void Configure(EntityTypeBuilder<AiContentWorkflowSession> builder)
    {
        builder.ToTable("ai_content_workflow_sessions");
        builder.HasKey(session => session.Id);
        builder.Property(session => session.Id).HasColumnName("id");
        builder.Property(session => session.IdeaId).HasColumnName("idea_id");
        builder.Property(session => session.CurrentStep)
            .HasColumnName("current_step")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(session => session.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(session => session.LinkedContentId).HasColumnName("linked_content_id");
        builder.Property(session => session.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(session => session.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.HasIndex(session => session.IdeaId).HasDatabaseName("ix_ai_content_workflow_sessions_idea_id");
        builder.HasIndex(session => session.CreatedByUserId).HasDatabaseName("ix_ai_content_workflow_sessions_created_by");
        builder.HasIndex(session => session.UpdatedAtUtc).HasDatabaseName("ix_ai_content_workflow_sessions_updated");
        builder.Ignore(session => session.DomainEvents);
        builder.Ignore(session => session.HasDomainEvents);
    }
}
