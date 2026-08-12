using HelpDev.Modules.Learning.Domain.Enrollments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HelpDev.Modules.Learning.Infrastructure.Persistence;

public sealed class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("enrollments");

        builder.HasKey(enrollment => enrollment.Id);

        builder.Property(enrollment => enrollment.Id)
            .ValueGeneratedNever();

        builder.Property(enrollment => enrollment.CourseId)
            .IsRequired()
            .HasColumnName("course_id");

        builder.Property(enrollment => enrollment.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(enrollment => enrollment.EnrolledAt)
            .IsRequired()
            .HasColumnName("enrolled_at");

        builder.Property(enrollment => enrollment.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("status");

        var progressConverter = new ValueConverter<ProgressPercentage, int>(
            progress => progress.Value,
            value => ProgressPercentage.FromPersisted(value));

        builder.Property(enrollment => enrollment.ProgressPercentage)
            .HasConversion(progressConverter)
            .IsRequired()
            .HasColumnName("progress_percentage");

        builder.HasIndex(enrollment => new { enrollment.CourseId, enrollment.UserId })
            .IsUnique();

        builder.HasMany(enrollment => enrollment.LessonProgressEntries)
            .WithOne()
            .HasForeignKey("EnrollmentId")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Navigation(enrollment => enrollment.LessonProgressEntries)
            .HasField("_lessonProgress")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
