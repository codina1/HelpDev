using HelpDev.Modules.Learning.Domain.Personalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Learning.Infrastructure.Persistence;

public sealed class LearningProfileConfiguration : IEntityTypeConfiguration<LearningProfile>
{
    public void Configure(EntityTypeBuilder<LearningProfile> builder)
    {
        builder.ToTable("learning_profiles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.UserId).IsRequired().HasColumnName("user_id");
        builder.HasIndex(x => x.UserId).IsUnique().HasDatabaseName("ix_learning_profiles_user_id");
        builder.Property(x => x.ExperienceLevel)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired()
            .HasColumnName("experience_level");
        builder.Property(x => x.LearningGoals)
            .IsRequired()
            .HasMaxLength(2000)
            .HasColumnName("learning_goals");
        builder.Property(x => x.CurrentSkills)
            .IsRequired()
            .HasMaxLength(1000)
            .HasColumnName("current_skills");
        builder.Property(x => x.CreatedAtUtc).IsRequired().HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).IsRequired().HasColumnName("updated_at_utc");

        builder.HasMany(x => x.Preferences)
            .WithOne()
            .HasForeignKey(x => x.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Preferences)
            .HasField("_preferences")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class LearningPreferenceConfiguration : IEntityTypeConfiguration<LearningPreference>
{
    public void Configure(EntityTypeBuilder<LearningPreference> builder)
    {
        builder.ToTable("learning_preferences");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.ProfileId).IsRequired().HasColumnName("profile_id");
        builder.Property(x => x.Topic).IsRequired().HasMaxLength(64).HasColumnName("topic");
        builder.Property(x => x.Priority).IsRequired().HasColumnName("priority");
        builder.Property(x => x.InterestLevel).IsRequired().HasColumnName("interest_level");
        builder.Property(x => x.SortOrder).IsRequired().HasColumnName("sort_order");
        builder.HasIndex(x => new { x.ProfileId, x.Topic })
            .IsUnique()
            .HasDatabaseName("ix_learning_preferences_profile_topic");
    }
}

public sealed class LearningRoadmapConfiguration : IEntityTypeConfiguration<LearningRoadmap>
{
    public void Configure(EntityTypeBuilder<LearningRoadmap> builder)
    {
        builder.ToTable("learning_roadmaps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.UserId).IsRequired().HasColumnName("user_id");
        builder.HasIndex(x => x.UserId).IsUnique().HasDatabaseName("ix_learning_roadmaps_user_id");
        builder.Property(x => x.Goal).IsRequired().HasMaxLength(200).HasColumnName("goal");
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired()
            .HasColumnName("status");
        builder.Property(x => x.CreatedAtUtc).IsRequired().HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).IsRequired().HasColumnName("updated_at_utc");
        builder.Property(x => x.ApprovedAtUtc).HasColumnName("approved_at_utc");

        builder.HasMany(x => x.Steps)
            .WithOne()
            .HasForeignKey(x => x.RoadmapId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Steps)
            .HasField("_steps")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class LearningRoadmapStepConfiguration : IEntityTypeConfiguration<LearningRoadmapStep>
{
    public void Configure(EntityTypeBuilder<LearningRoadmapStep> builder)
    {
        builder.ToTable("learning_roadmap_steps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.RoadmapId).IsRequired().HasColumnName("roadmap_id");
        builder.Property(x => x.StepOrder).IsRequired().HasColumnName("step_order");
        builder.Property(x => x.Title).IsRequired().HasMaxLength(160).HasColumnName("title");
        builder.Property(x => x.Description).IsRequired().HasMaxLength(1000).HasColumnName("description");
        builder.Property(x => x.RelatedCourseId).HasColumnName("related_course_id");
        builder.HasIndex(x => new { x.RoadmapId, x.StepOrder })
            .IsUnique()
            .HasDatabaseName("ix_learning_roadmap_steps_order");
    }
}
