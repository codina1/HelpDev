using HelpDev.Modules.Learning.Application.Enrollments.Dtos;
using HelpDev.Modules.Learning.Application.Persistence;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.Learning.Domain.Enrollments;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;

namespace HelpDev.Modules.Learning.Application.Enrollments;

public sealed class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IEnrollmentQueries _enrollmentQueries;
    private readonly ICourseLearningQueries _courseLearningQueries;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepository,
        IEnrollmentQueries enrollmentQueries,
        ICourseLearningQueries courseLearningQueries,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock)
    {
        _enrollmentRepository = enrollmentRepository;
        _enrollmentQueries = enrollmentQueries;
        _courseLearningQueries = courseLearningQueries;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<EnrollmentDto> EnrollAsync(
        EnrollStudentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureCourseId(request.CourseId);
        EnsureUserId(request.UserId);

        var structure = await GetRequiredStructureAsync(request.CourseId, cancellationToken);
        EnsurePublishedWithLessons(structure);

        var existing = await _enrollmentRepository.GetByCourseAndUserAsync(
            request.CourseId,
            request.UserId,
            cancellationToken);
        if (existing is not null)
        {
            throw new EnrollmentException(
                "Student is already enrolled in this course.",
                EnrollmentErrorCodes.AlreadyExists);
        }

        try
        {
            var enrollment = Enrollment.Enroll(
                Guid.NewGuid(),
                request.CourseId,
                request.UserId,
                _clock.UtcNow);

            await _enrollmentRepository.AddAsync(enrollment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return EnrollmentMapper.ToDto(enrollment);
        }
        catch (DomainException ex)
        {
            throw WrapDomainException(ex);
        }
    }

    public async Task<EnrollmentDto> GetByIdAsync(
        Guid enrollmentId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        EnsureUserId(userId);

        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId, cancellationToken);
        if (enrollment is null || enrollment.UserId != userId)
        {
            throw new EnrollmentException("Enrollment was not found.", EnrollmentErrorCodes.NotFound);
        }

        return EnrollmentMapper.ToDto(enrollment);
    }

    public async Task<EnrollmentDto> GetByCourseAndUserAsync(
        Guid courseId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        EnsureCourseId(courseId);
        EnsureUserId(userId);

        var enrollment = await _enrollmentRepository.GetByCourseAndUserAsync(
            courseId,
            userId,
            cancellationToken);
        if (enrollment is null)
        {
            throw new EnrollmentException("Enrollment was not found.", EnrollmentErrorCodes.NotFound);
        }

        return EnrollmentMapper.ToDto(enrollment);
    }

    public Task<IReadOnlyList<EnrollmentListItemDto>> ListByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        EnsureUserId(userId);
        return _enrollmentQueries.ListByUserAsync(userId, cancellationToken);
    }

    public async Task<EnrollmentDto> StartLessonAsync(
        StartLessonRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureCourseId(request.CourseId);
        EnsureUserId(request.UserId);
        EnsureLessonId(request.LessonId);

        var enrollment = await GetRequiredEnrollmentAsync(
            request.CourseId,
            request.UserId,
            cancellationToken);
        var structure = await GetRequiredStructureAsync(request.CourseId, cancellationToken);
        EnsureLessonInCourse(structure, request.LessonId);

        try
        {
            enrollment.StartLesson(request.LessonId, _clock.UtcNow);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return EnrollmentMapper.ToDto(enrollment);
        }
        catch (DomainException ex)
        {
            throw WrapDomainException(ex);
        }
    }

    public async Task<EnrollmentDto> CompleteLessonAsync(
        CompleteLessonRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureCourseId(request.CourseId);
        EnsureUserId(request.UserId);
        EnsureLessonId(request.LessonId);

        var enrollment = await GetRequiredEnrollmentAsync(
            request.CourseId,
            request.UserId,
            cancellationToken);
        var structure = await GetRequiredStructureAsync(request.CourseId, cancellationToken);
        EnsureLessonInCourse(structure, request.LessonId);

        try
        {
            enrollment.CompleteLesson(request.LessonId, structure.LessonIds, _clock.UtcNow);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return EnrollmentMapper.ToDto(enrollment);
        }
        catch (DomainException ex)
        {
            throw WrapDomainException(ex);
        }
    }

    private async Task<Enrollment> GetRequiredEnrollmentAsync(
        Guid courseId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentRepository.GetByCourseAndUserAsync(
            courseId,
            userId,
            cancellationToken);
        if (enrollment is null)
        {
            throw new EnrollmentException("Enrollment was not found.", EnrollmentErrorCodes.NotFound);
        }

        return enrollment;
    }

    private async Task<CourseLearningStructure> GetRequiredStructureAsync(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var structure = await _courseLearningQueries.GetStructureAsync(courseId, cancellationToken);
        if (structure is null)
        {
            throw new EnrollmentException("Course was not found.", EnrollmentErrorCodes.CourseNotFound);
        }

        return structure;
    }

    private static void EnsurePublishedWithLessons(CourseLearningStructure structure)
    {
        if (structure.Status != CourseStatus.Published)
        {
            throw new EnrollmentException(
                "Course must be published before enrollment.",
                EnrollmentErrorCodes.CourseNotPublished);
        }

        if (structure.LessonIds.Count == 0)
        {
            throw new EnrollmentException(
                "Course has no lessons.",
                EnrollmentErrorCodes.CourseHasNoLessons);
        }
    }

    private static void EnsureLessonInCourse(CourseLearningStructure structure, Guid lessonId)
    {
        if (!structure.LessonIds.Contains(lessonId))
        {
            throw new EnrollmentException(
                "Lesson does not belong to the course.",
                EnrollmentErrorCodes.LessonNotInCourse);
        }
    }

    private static void EnsureCourseId(Guid courseId)
    {
        if (courseId == Guid.Empty)
        {
            throw new EnrollmentException("Course id must not be empty.", EnrollmentErrorCodes.CourseInvalid);
        }
    }

    private static void EnsureUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new EnrollmentException("User id must not be empty.", EnrollmentErrorCodes.UserInvalid);
        }
    }

    private static void EnsureLessonId(Guid lessonId)
    {
        if (lessonId == Guid.Empty)
        {
            throw new EnrollmentException("Lesson id must not be empty.", EnrollmentErrorCodes.LessonInvalid);
        }
    }

    private static EnrollmentException WrapDomainException(DomainException exception) =>
        new(exception.Message, EnrollmentErrorCodes.OperationInvalid, exception);
}
