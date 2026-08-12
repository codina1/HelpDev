using HelpDev.Modules.Learning.Domain.Enrollments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Learning.Infrastructure.Persistence;

public sealed class LessonProgressConfiguration : IEntityTypeConfiguration<LessonProgress>
{
    public void Configure(EntityTypeBuilder<LessonProgress> builder)
    {
        builder.ToTable("lesson_progresses");

        builder.Property(progress => progress.LessonId)
            .IsRequired()
            .HasColumnName("lesson_id");

        builder.Property(progress => progress.StartedAt)
            .HasColumnName("started_at");

        builder.Property(progress => progress.CompletedAt)
            .HasColumnName("completed_at");

        builder.Ignore(progress => progress.IsCompleted);

        builder.Property<Guid>("EnrollmentId")
            .HasColumnName("enrollment_id");

        builder.HasKey("EnrollmentId", nameof(LessonProgress.LessonId));

        builder.HasIndex("EnrollmentId", nameof(LessonProgress.LessonId))
            .IsUnique();
    }
}
