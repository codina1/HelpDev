using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Domain.Courses;

namespace HelpDev.Modules.Learning.Application.Courses;

public static class CourseMapper
{
    public static CourseDetailDto ToDetailDto(Course course) =>
        new(
            course.Id,
            course.Title,
            course.Slug.Value,
            course.Description,
            course.InstructorId,
            course.Status.ToString(),
            course.CreatedAt,
            course.PublishedAt,
            course.Sections
                .OrderBy(section => section.Order)
                .Select(ToSectionDto)
                .ToList());

    public static CourseSectionDto ToSectionDto(Section section) =>
        new(
            section.Id,
            section.Title,
            section.Order,
            section.Lessons
                .OrderBy(lesson => lesson.Order)
                .Select(ToLessonDto)
                .ToList());

    public static CourseLessonDto ToLessonDto(Lesson lesson) =>
        new(
            lesson.Id,
            lesson.Title,
            lesson.Order,
            lesson.ContentId,
            lesson.VideoUrl,
            lesson.DurationMinutes,
            lesson.IsPreview);
}
