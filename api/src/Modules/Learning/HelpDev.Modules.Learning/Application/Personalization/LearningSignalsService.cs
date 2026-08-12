using HelpDev.Modules.Learning.Application.Persistence;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.Learning.Domain.Enrollments;
using HelpDev.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Learning.Application.Personalization;

/// <summary>
/// Builds learning signals from existing enrollments/progress only — no invented likes/shares/scores.
/// Content-linked completions count lessons that reference a ContentId.
/// </summary>
public sealed class LearningSignalsService : ILearningSignalsService
{
    private readonly ILearningDbContext _db;
    private readonly IDateTimeProvider _clock;

    public LearningSignalsService(ILearningDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<LearningSignalsDto> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new LearningPersonalizationException(
                "شناسه کاربر نامعتبر است.",
                LearningPersonalizationErrorCodes.InvalidUser);
        }

        var enrollments = await _db.Enrollments
            .AsNoTracking()
            .Include(e => e.LessonProgressEntries)
            .Where(e => e.UserId == userId)
            .ToListAsync(cancellationToken);

        var courseIds = enrollments.Select(e => e.CourseId).Distinct().ToList();
        var courses = await _db.Courses
            .AsNoTracking()
            .Include(c => c.Sections)
            .ThenInclude(s => s.Lessons)
            .Where(c => courseIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        var courseMap = courses.ToDictionary(c => c.Id, c => c);
        var contentLinkedLessonIds = courses
            .SelectMany(c => c.Sections.SelectMany(s => s.Lessons))
            .Where(l => l.ContentId.HasValue)
            .Select(l => l.Id)
            .ToHashSet();

        var enrollmentDtos = new List<LearningSignalEnrollmentDto>();
        var completedLessons = 0;
        var contentLinkedCompletions = 0;

        foreach (var enrollment in enrollments.OrderByDescending(e => e.EnrolledAt))
        {
            var completed = enrollment.LessonProgressEntries.Count(p => p.IsCompleted);
            completedLessons += completed;
            contentLinkedCompletions += enrollment.LessonProgressEntries
                .Count(p => p.IsCompleted && contentLinkedLessonIds.Contains(p.LessonId));

            var title = courseMap.TryGetValue(enrollment.CourseId, out var course)
                ? course.Title
                : enrollment.CourseId.ToString("D");

            enrollmentDtos.Add(new LearningSignalEnrollmentDto(
                enrollment.CourseId,
                title,
                enrollment.Status.ToString(),
                enrollment.ProgressPercentage.Value,
                completed));
        }

        return new LearningSignalsDto(
            userId,
            enrollments.Count,
            enrollments.Count(e => e.Status == EnrollmentStatus.Active),
            enrollments.Count(e => e.Status == EnrollmentStatus.Completed),
            completedLessons,
            contentLinkedCompletions,
            enrollmentDtos,
            _clock.UtcNow);
    }
}
