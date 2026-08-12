using HelpDev.Modules.Learning.Domain.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HelpDev.Modules.Learning.Infrastructure.Persistence;

public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("courses");

        builder.HasKey(course => course.Id);

        builder.Property(course => course.Id)
            .ValueGeneratedNever();

        builder.Property(course => course.Title)
            .IsRequired()
            .HasMaxLength(300)
            .HasColumnName("title");

        var slugConverter = new ValueConverter<CourseSlug, string>(
            slug => slug.Value,
            value => CourseSlug.FromPersisted(value));

        builder.Property(course => course.Slug)
            .HasConversion(slugConverter)
            .IsRequired()
            .HasMaxLength(300)
            .HasColumnName("slug");

        builder.HasIndex(course => course.Slug)
            .IsUnique();

        builder.Property(course => course.Description)
            .IsRequired()
            .HasColumnType("text")
            .HasColumnName("description");

        builder.Property(course => course.InstructorId)
            .IsRequired()
            .HasColumnName("instructor_id");

        builder.Property(course => course.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("status");

        builder.Property(course => course.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(course => course.PublishedAt)
            .HasColumnName("published_at");

        builder.HasMany(course => course.Sections)
            .WithOne()
            .HasForeignKey("CourseId")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Navigation(course => course.Sections)
            .HasField("_sections")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
