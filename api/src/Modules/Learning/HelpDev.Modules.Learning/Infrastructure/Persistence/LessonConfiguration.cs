using HelpDev.Modules.Learning.Domain.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Learning.Infrastructure.Persistence;

public sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("course_lessons");

        builder.HasKey(lesson => lesson.Id);

        builder.Property(lesson => lesson.Id)
            .ValueGeneratedNever();

        builder.Property(lesson => lesson.Title)
            .IsRequired()
            .HasMaxLength(300)
            .HasColumnName("title");

        builder.Property(lesson => lesson.Order)
            .IsRequired()
            .HasColumnName("order");

        builder.Property(lesson => lesson.ContentId)
            .HasColumnName("content_id");

        builder.Property(lesson => lesson.VideoUrl)
            .HasMaxLength(2000)
            .HasColumnName("video_url");

        builder.Property(lesson => lesson.DurationMinutes)
            .HasColumnName("duration_minutes");

        builder.Property(lesson => lesson.IsPreview)
            .IsRequired()
            .HasColumnName("is_preview");

        builder.Property<Guid>("SectionId")
            .HasColumnName("section_id");

        builder.HasIndex("SectionId", nameof(Lesson.Order))
            .IsUnique();
    }
}
