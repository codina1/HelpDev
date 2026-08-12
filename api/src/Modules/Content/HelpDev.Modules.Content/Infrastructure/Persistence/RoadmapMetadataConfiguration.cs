using HelpDev.Modules.Content.Domain.Roadmaps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

public sealed class RoadmapMetadataConfiguration : IEntityTypeConfiguration<RoadmapMetadata>
{
    public void Configure(EntityTypeBuilder<RoadmapMetadata> builder)
    {
        builder.ToTable("roadmap_metadata");

        builder.HasKey(roadmap => roadmap.Id);
        builder.Property(roadmap => roadmap.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(roadmap => roadmap.ContentId).HasColumnName("content_id");
        builder.Property(roadmap => roadmap.Level)
            .HasColumnName("level")
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(roadmap => roadmap.EstimatedDuration)
            .HasColumnName("estimated_duration")
            .HasMaxLength(RoadmapMetadata.MaxEstimatedDurationLength)
            .IsRequired();
        builder.Property(roadmap => roadmap.Goal)
            .HasColumnName("goal")
            .HasMaxLength(RoadmapMetadata.MaxGoalLength)
            .IsRequired();
        builder.Property(roadmap => roadmap.Prerequisites)
            .HasColumnName("prerequisites")
            .HasMaxLength(RoadmapMetadata.MaxPrerequisitesLength);
        builder.Property(roadmap => roadmap.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(roadmap => roadmap.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(roadmap => roadmap.ContentId)
            .IsUnique()
            .HasDatabaseName("ix_roadmap_metadata_content_id");

        builder.HasOne<ContentEntity>()
            .WithMany()
            .HasForeignKey(roadmap => roadmap.ContentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_roadmap_metadata_contents_content_id");

        builder.HasMany(roadmap => roadmap.Steps)
            .WithOne()
            .HasForeignKey(step => step.RoadmapId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(roadmap => roadmap.Steps)
            .HasField("_steps")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class RoadmapStepConfiguration : IEntityTypeConfiguration<RoadmapStep>
{
    public void Configure(EntityTypeBuilder<RoadmapStep> builder)
    {
        builder.ToTable("roadmap_steps");
        builder.HasKey(step => step.Id);
        builder.Property(step => step.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(step => step.RoadmapId).HasColumnName("roadmap_id");
        builder.Property(step => step.Title)
            .HasColumnName("title")
            .HasMaxLength(RoadmapStep.MaxTitleLength)
            .IsRequired();
        builder.Property(step => step.Description)
            .HasColumnName("description")
            .HasMaxLength(RoadmapStep.MaxDescriptionLength);
        builder.Property(step => step.Order).HasColumnName("sort_order");
        builder.Property(step => step.EstimatedHours).HasColumnName("estimated_hours");
        builder.Property(step => step.ProjectTitle)
            .HasColumnName("project_title")
            .HasMaxLength(RoadmapStep.MaxProjectTitleLength);
        builder.Property(step => step.ProjectDescription)
            .HasColumnName("project_description")
            .HasMaxLength(RoadmapStep.MaxProjectDescriptionLength);

        builder.HasIndex(step => step.RoadmapId)
            .HasDatabaseName("ix_roadmap_steps_roadmap_id");

        builder.HasIndex(step => new { step.RoadmapId, step.Order })
            .HasDatabaseName("ix_roadmap_steps_roadmap_id_sort_order");

        builder.HasMany(step => step.Topics)
            .WithOne()
            .HasForeignKey(topic => topic.StepId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(step => step.Resources)
            .WithOne()
            .HasForeignKey(resource => resource.StepId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(step => step.Topics)
            .HasField("_topics")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(step => step.Resources)
            .HasField("_resources")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class RoadmapTopicConfiguration : IEntityTypeConfiguration<RoadmapTopic>
{
    public void Configure(EntityTypeBuilder<RoadmapTopic> builder)
    {
        builder.ToTable("roadmap_topics");
        builder.HasKey(topic => topic.Id);
        builder.Property(topic => topic.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(topic => topic.StepId).HasColumnName("step_id");
        builder.Property(topic => topic.Title)
            .HasColumnName("title")
            .HasMaxLength(RoadmapTopic.MaxTitleLength)
            .IsRequired();
        builder.Property(topic => topic.Description)
            .HasColumnName("description")
            .HasMaxLength(RoadmapTopic.MaxDescriptionLength);
        builder.Property(topic => topic.Order).HasColumnName("sort_order");

        builder.HasIndex(topic => topic.StepId)
            .HasDatabaseName("ix_roadmap_topics_step_id");

        builder.HasIndex(topic => new { topic.StepId, topic.Order })
            .HasDatabaseName("ix_roadmap_topics_step_id_sort_order");
    }
}

public sealed class RoadmapResourceConfiguration : IEntityTypeConfiguration<RoadmapResource>
{
    public void Configure(EntityTypeBuilder<RoadmapResource> builder)
    {
        builder.ToTable("roadmap_resources");
        builder.HasKey(resource => resource.Id);
        builder.Property(resource => resource.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(resource => resource.StepId).HasColumnName("step_id");
        builder.Property(resource => resource.Title)
            .HasColumnName("title")
            .HasMaxLength(RoadmapResource.MaxTitleLength)
            .IsRequired();
        builder.Property(resource => resource.Url)
            .HasColumnName("url")
            .HasMaxLength(RoadmapResource.MaxUrlLength)
            .IsRequired();
        builder.Property(resource => resource.ResourceType)
            .HasColumnName("resource_type")
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(resource => resource.Order).HasColumnName("sort_order");

        builder.HasIndex(resource => resource.StepId)
            .HasDatabaseName("ix_roadmap_resources_step_id");

        builder.HasIndex(resource => new { resource.StepId, resource.Order })
            .HasDatabaseName("ix_roadmap_resources_step_id_sort_order");
    }
}
