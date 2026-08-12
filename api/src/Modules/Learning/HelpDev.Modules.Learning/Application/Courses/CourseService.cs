using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Application.Persistence;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Analytics;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging;

namespace HelpDev.Modules.Learning.Application.Courses;

public sealed class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly ICourseQueries _courseQueries;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly IAnalyticsEventIngestor _analyticsIngestor;
    private readonly ILogger<CourseService> _logger;

    public CourseService(
        ICourseRepository courseRepository,
        ICourseQueries courseQueries,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock,
        IAnalyticsEventIngestor analyticsIngestor,
        ILogger<CourseService> logger)
    {
        _courseRepository = courseRepository;
        _courseQueries = courseQueries;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _analyticsIngestor = analyticsIngestor;
        _logger = logger;
    }

    public async Task<CourseDetailDto> CreateAsync(
        CourseManagementActor actor,
        CreateCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(request);

        // InstructorId always comes from the authenticated actor, never CanManageAllCourses.
        var instructorId = actor.UserId;
        var slug = CreateSlugOrThrow(request.Slug);

        if (await _courseRepository.SlugExistsAsync(slug, excludingCourseId: null, cancellationToken))
        {
            throw new CourseException("Course slug is already in use.", CourseErrorCodes.SlugDuplicate);
        }

        try
        {
            var course = Course.CreateDraft(
                Guid.NewGuid(),
                request.Title,
                slug,
                request.Description,
                instructorId,
                _clock.UtcNow);

            await _courseRepository.AddAsync(course, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await TryIngestCourseCreatedAsync(course, cancellationToken);
            return CourseMapper.ToDetailDto(course);
        }
        catch (DomainException ex)
        {
            throw WrapDomainException(ex);
        }
    }

    public async Task<CourseDetailDto> GetByIdAsync(
        CourseManagementActor actor,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var course = await GetManagedCourseAsync(actor, courseId, cancellationToken);
        return CourseMapper.ToDetailDto(course);
    }

    public async Task<CourseDetailDto> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var courseSlug = CreateSlugOrThrow(slug);
        var course = await _courseRepository.GetBySlugAsync(courseSlug.Value, cancellationToken);
        if (course is null)
        {
            throw new CourseException("Course was not found.", CourseErrorCodes.NotFound);
        }

        return CourseMapper.ToDetailDto(course);
    }

    public Task<IReadOnlyList<CourseListItemDto>> ListAsync(
        CourseManagementActor actor,
        CourseStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);

        Guid? instructorId = actor.CanManageAllCourses ? null : actor.UserId;
        return _courseQueries.ListAsync(status, instructorId, cancellationToken);
    }

    public async Task<CourseDetailDto> UpdateDetailsAsync(
        CourseManagementActor actor,
        Guid courseId,
        UpdateCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var course = await GetManagedCourseAsync(actor, courseId, cancellationToken);
        var slug = CreateSlugOrThrow(request.Slug);

        if (slug != course.Slug
            && await _courseRepository.SlugExistsAsync(slug, course.Id, cancellationToken))
        {
            throw new CourseException("Course slug is already in use.", CourseErrorCodes.SlugDuplicate);
        }

        try
        {
            course.UpdateDetails(request.Title, slug, request.Description);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return CourseMapper.ToDetailDto(course);
        }
        catch (DomainException ex)
        {
            throw WrapDomainException(ex);
        }
    }

    public async Task<CourseDetailDto> AddSectionAsync(
        CourseManagementActor actor,
        Guid courseId,
        AddSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var course = await GetManagedCourseAsync(actor, courseId, cancellationToken);

        try
        {
            course.AddSection(Guid.NewGuid(), request.Title);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return CourseMapper.ToDetailDto(course);
        }
        catch (DomainException ex)
        {
            throw WrapDomainException(ex);
        }
    }

    public async Task<CourseDetailDto> RenameSectionAsync(
        CourseManagementActor actor,
        Guid courseId,
        RenameSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var course = await GetManagedCourseAsync(actor, courseId, cancellationToken);

        try
        {
            course.RenameSection(request.SectionId, request.Title);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return CourseMapper.ToDetailDto(course);
        }
        catch (DomainException ex)
        {
            throw WrapDomainException(ex);
        }
    }

    public async Task<CourseDetailDto> ReorderSectionAsync(
        CourseManagementActor actor,
        Guid courseId,
        ReorderSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var course = await GetManagedCourseAsync(actor, courseId, cancellationToken);

        try
        {
            course.ReorderSection(request.SectionId, request.NewOrder);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return CourseMapper.ToDetailDto(course);
        }
        catch (DomainException ex)
        {
            throw WrapDomainException(ex);
        }
    }

    public async Task<CourseDetailDto> AddLessonAsync(
        CourseManagementActor actor,
        Guid courseId,
        AddLessonRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var course = await GetManagedCourseAsync(actor, courseId, cancellationToken);

        try
        {
            course.AddLesson(
                request.SectionId,
                Guid.NewGuid(),
                request.Title,
                request.ContentId,
                request.VideoUrl,
                request.DurationMinutes,
                request.IsPreview);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return CourseMapper.ToDetailDto(course);
        }
        catch (DomainException ex)
        {
            throw WrapDomainException(ex);
        }
    }

    public async Task<CourseDetailDto> UpdateLessonAsync(
        CourseManagementActor actor,
        Guid courseId,
        UpdateLessonRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var course = await GetManagedCourseAsync(actor, courseId, cancellationToken);

        try
        {
            course.UpdateLesson(
                request.SectionId,
                request.LessonId,
                request.Title,
                request.ContentId,
                request.VideoUrl,
                request.DurationMinutes,
                request.IsPreview);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return CourseMapper.ToDetailDto(course);
        }
        catch (DomainException ex)
        {
            throw WrapDomainException(ex);
        }
    }

    public async Task<CourseDetailDto> ReorderLessonAsync(
        CourseManagementActor actor,
        Guid courseId,
        ReorderLessonRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var course = await GetManagedCourseAsync(actor, courseId, cancellationToken);

        try
        {
            course.ReorderLesson(request.SectionId, request.LessonId, request.NewOrder);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return CourseMapper.ToDetailDto(course);
        }
        catch (DomainException ex)
        {
            throw WrapDomainException(ex);
        }
    }

    public async Task<CourseDetailDto> PublishAsync(
        CourseManagementActor actor,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var course = await GetManagedCourseAsync(actor, courseId, cancellationToken);

        try
        {
            course.Publish(_clock.UtcNow);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return CourseMapper.ToDetailDto(course);
        }
        catch (DomainException ex)
        {
            throw WrapDomainException(ex);
        }
    }

    private async Task<Course> GetManagedCourseAsync(
        CourseManagementActor actor,
        Guid courseId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actor);

        var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken);
        if (course is null)
        {
            throw new CourseException("Course was not found.", CourseErrorCodes.NotFound);
        }

        EnsureCanManage(course, actor);
        return course;
    }

    /// <summary>
    /// Cross-owner access is indistinguishable from a missing course (course_not_found).
    /// </summary>
    public static void EnsureCanManage(Course course, CourseManagementActor actor)
    {
        ArgumentNullException.ThrowIfNull(course);
        ArgumentNullException.ThrowIfNull(actor);

        if (actor.CanManageAllCourses || course.InstructorId == actor.UserId)
        {
            return;
        }

        throw new CourseException("Course was not found.", CourseErrorCodes.NotFound);
    }

    private static CourseSlug CreateSlugOrThrow(string? slug)
    {
        if (!CourseSlug.TryCreate(slug, out var courseSlug) || courseSlug is null)
        {
            throw new CourseException("Course slug is invalid.", CourseErrorCodes.SlugInvalid);
        }

        return courseSlug;
    }

    private static CourseException WrapDomainException(DomainException exception) =>
        new(exception.Message, CourseErrorCodes.OperationInvalid, exception);

    private async Task TryIngestCourseCreatedAsync(Course course, CancellationToken cancellationToken)
    {
        try
        {
            await _analyticsIngestor.IngestAsync(
                new AnalyticsEventEnvelope(
                    Guid.NewGuid(),
                    AnalyticsEventTypes.LearningCourseCreated,
                    _clock.UtcNow,
                    course.InstructorId,
                    course.Id,
                    "Course",
                    Dimensions: null,
                    SubjectDisplayName: course.Title,
                    SubjectSlug: course.Slug.Value),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Analytics course created ingestion skipped.");
        }
    }
}
