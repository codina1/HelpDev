using HelpDev.Modules.Learning.Domain.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDev.Modules.Learning.Infrastructure.Persistence;

public sealed class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.ToTable("course_sections");

        builder.HasKey(section => section.Id);

        builder.Property(section => section.Id)
            .ValueGeneratedNever();

        builder.Property(section => section.Title)
            .IsRequired()
            .HasMaxLength(300)
            .HasColumnName("title");

        builder.Property(section => section.Order)
            .IsRequired()
            .HasColumnName("order");

        builder.Property<Guid>("CourseId")
            .HasColumnName("course_id");

        builder.HasIndex("CourseId", nameof(Section.Order))
            .IsUnique();

        builder.HasMany(section => section.Lessons)
            .WithOne()
            .HasForeignKey("SectionId")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Navigation(section => section.Lessons)
            .HasField("_lessons")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
