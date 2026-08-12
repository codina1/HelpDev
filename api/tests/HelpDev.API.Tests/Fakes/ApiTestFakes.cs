using System.Security.Claims;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Application.Enrollments;
using HelpDev.Modules.Learning.Application.Enrollments.Dtos;
using HelpDev.Modules.Learning.Domain.Courses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Tests.Fakes;

internal sealed class FakePublicCourseQueries : IPublicCourseQueries
{
    public IReadOnlyList<CourseListItemDto> PublishedList { get; set; } = [];

    public CourseDetailDto? PublishedById { get; set; }

    public CourseDetailDto? PublishedBySlug { get; set; }

    public Guid? LastGetById { get; private set; }

    public string? LastGetBySlug { get; private set; }

    public Task<IReadOnlyList<CourseListItemDto>> ListPublishedAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult(PublishedList);

    public Task<CourseDetailDto?> GetPublishedByIdAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        LastGetById = courseId;
        return Task.FromResult(PublishedById);
    }

    public Task<CourseDetailDto?> GetPublishedBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        LastGetBySlug = slug;
        return Task.FromResult(PublishedBySlug);
    }

    public Task<IReadOnlyList<CourseSearchSourceDto>> ListPublishedSearchBatchAsync(
        Guid? afterCourseId,
        int take,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<CourseSearchSourceDto>>([]);
}

internal sealed class FakeCourseService : ICourseService
{
    public CourseManagementActor? LastActor { get; private set; }

    public Guid? LastInstructorId => LastActor?.UserId;

    public CreateCourseRequest? LastCreateRequest { get; private set; }

    public Guid? LastCourseId { get; private set; }

    public Guid? LastSectionId { get; private set; }

    public Guid? LastLessonId { get; private set; }

    public string? LastOperation { get; private set; }

    public CourseDetailDto CourseToReturn { get; set; } = CreateSampleDetail();

    public Exception? ExceptionToThrow { get; set; }

    public Task<CourseDetailDto> CreateAsync(
        CourseManagementActor actor,
        CreateCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNeeded();
        LastActor = actor;
        LastCreateRequest = request;
        LastOperation = nameof(CreateAsync);
        return Task.FromResult(CourseToReturn with { InstructorId = actor.UserId });
    }

    public Task<CourseDetailDto> GetByIdAsync(
        CourseManagementActor actor,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNeeded();
        LastActor = actor;
        LastCourseId = courseId;
        LastOperation = nameof(GetByIdAsync);
        return Task.FromResult(CourseToReturn);
    }

    public Task<CourseDetailDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<IReadOnlyList<CourseListItemDto>> ListAsync(
        CourseManagementActor actor,
        CourseStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastOperation = nameof(ListAsync);
        return Task.FromResult<IReadOnlyList<CourseListItemDto>>(
        [
            new CourseListItemDto(
                CourseToReturn.Id,
                CourseToReturn.Title,
                CourseToReturn.Slug,
                status?.ToString() ?? CourseToReturn.Status,
                CourseToReturn.InstructorId,
                CourseToReturn.CreatedAt,
                CourseToReturn.PublishedAt,
                0,
                0),
        ]);
    }

    public Task<CourseDetailDto> UpdateDetailsAsync(
        CourseManagementActor actor,
        Guid courseId,
        UpdateCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNeeded();
        LastActor = actor;
        LastCourseId = courseId;
        LastOperation = nameof(UpdateDetailsAsync);
        return Task.FromResult(CourseToReturn);
    }

    public Task<CourseDetailDto> AddSectionAsync(
        CourseManagementActor actor,
        Guid courseId,
        AddSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastCourseId = courseId;
        LastOperation = nameof(AddSectionAsync);
        return Task.FromResult(CourseToReturn);
    }

    public Task<CourseDetailDto> RenameSectionAsync(
        CourseManagementActor actor,
        Guid courseId,
        RenameSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastCourseId = courseId;
        LastSectionId = request.SectionId;
        LastOperation = nameof(RenameSectionAsync);
        return Task.FromResult(CourseToReturn);
    }

    public Task<CourseDetailDto> ReorderSectionAsync(
        CourseManagementActor actor,
        Guid courseId,
        ReorderSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastCourseId = courseId;
        LastSectionId = request.SectionId;
        LastOperation = nameof(ReorderSectionAsync);
        return Task.FromResult(CourseToReturn);
    }

    public Task<CourseDetailDto> AddLessonAsync(
        CourseManagementActor actor,
        Guid courseId,
        AddLessonRequest request,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastCourseId = courseId;
        LastSectionId = request.SectionId;
        LastOperation = nameof(AddLessonAsync);
        return Task.FromResult(CourseToReturn);
    }

    public Task<CourseDetailDto> UpdateLessonAsync(
        CourseManagementActor actor,
        Guid courseId,
        UpdateLessonRequest request,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastCourseId = courseId;
        LastSectionId = request.SectionId;
        LastLessonId = request.LessonId;
        LastOperation = nameof(UpdateLessonAsync);
        return Task.FromResult(CourseToReturn);
    }

    public Task<CourseDetailDto> ReorderLessonAsync(
        CourseManagementActor actor,
        Guid courseId,
        ReorderLessonRequest request,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastCourseId = courseId;
        LastSectionId = request.SectionId;
        LastLessonId = request.LessonId;
        LastOperation = nameof(ReorderLessonAsync);
        return Task.FromResult(CourseToReturn);
    }

    public Task<CourseDetailDto> PublishAsync(
        CourseManagementActor actor,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNeeded();
        LastActor = actor;
        LastCourseId = courseId;
        LastOperation = nameof(PublishAsync);
        return Task.FromResult(CourseToReturn);
    }

    private void ThrowIfNeeded()
    {
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }
    }

    internal static CourseDetailDto CreateSampleDetail() =>
        new(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Sample",
            "sample-course",
            "Desc",
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            nameof(CourseStatus.Draft),
            DateTime.UtcNow,
            null,
            Array.Empty<CourseSectionDto>());
}

internal sealed class FakeEnrollmentService : IEnrollmentService
{
    public EnrollmentDto EnrollmentToReturn { get; set; } = CreateSampleEnrollment();

    public IReadOnlyList<EnrollmentListItemDto> ListToReturn { get; set; } = [];

    public Exception? ExceptionToThrow { get; set; }

    public string? LastOperation { get; private set; }

    public Guid? LastCourseId { get; private set; }

    public Guid? LastUserId { get; private set; }

    public Guid? LastEnrollmentId { get; private set; }

    public Guid? LastLessonId { get; private set; }

    public CancellationToken LastCancellationToken { get; private set; }

    public EnrollStudentRequest? LastEnrollRequest { get; private set; }

    public StartLessonRequest? LastStartLessonRequest { get; private set; }

    public CompleteLessonRequest? LastCompleteLessonRequest { get; private set; }

    public Task<EnrollmentDto> EnrollAsync(
        EnrollStudentRequest request,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNeeded();
        LastOperation = nameof(EnrollAsync);
        LastEnrollRequest = request;
        LastCourseId = request.CourseId;
        LastUserId = request.UserId;
        LastCancellationToken = cancellationToken;
        return Task.FromResult(EnrollmentToReturn with
        {
            CourseId = request.CourseId,
            UserId = request.UserId,
        });
    }

    public Task<EnrollmentDto> GetByIdAsync(
        Guid enrollmentId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNeeded();
        LastOperation = nameof(GetByIdAsync);
        LastEnrollmentId = enrollmentId;
        LastUserId = userId;
        LastCancellationToken = cancellationToken;
        return Task.FromResult(EnrollmentToReturn);
    }

    public Task<EnrollmentDto> GetByCourseAndUserAsync(
        Guid courseId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNeeded();
        LastOperation = nameof(GetByCourseAndUserAsync);
        LastCourseId = courseId;
        LastUserId = userId;
        LastCancellationToken = cancellationToken;
        return Task.FromResult(EnrollmentToReturn);
    }

    public Task<IReadOnlyList<EnrollmentListItemDto>> ListByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNeeded();
        LastOperation = nameof(ListByUserAsync);
        LastUserId = userId;
        LastCancellationToken = cancellationToken;
        return Task.FromResult(ListToReturn);
    }

    public Task<EnrollmentDto> StartLessonAsync(
        StartLessonRequest request,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNeeded();
        LastOperation = nameof(StartLessonAsync);
        LastStartLessonRequest = request;
        LastCourseId = request.CourseId;
        LastUserId = request.UserId;
        LastLessonId = request.LessonId;
        LastCancellationToken = cancellationToken;
        return Task.FromResult(EnrollmentToReturn);
    }

    public Task<EnrollmentDto> CompleteLessonAsync(
        CompleteLessonRequest request,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNeeded();
        LastOperation = nameof(CompleteLessonAsync);
        LastCompleteLessonRequest = request;
        LastCourseId = request.CourseId;
        LastUserId = request.UserId;
        LastLessonId = request.LessonId;
        LastCancellationToken = cancellationToken;
        return Task.FromResult(EnrollmentToReturn);
    }

    private void ThrowIfNeeded()
    {
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }
    }

    internal static EnrollmentDto CreateSampleEnrollment() =>
        new(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            DateTime.UtcNow,
            "Active",
            0,
            Array.Empty<LessonProgressDto>());
}

internal static class ControllerTestHelper
{
    public static void SetUser(ControllerBase controller, Guid? userId, params string[] roles)
    {
        var claims = new List<Claim>();
        if (userId.HasValue)
        {
            claims.Add(new Claim(JwtClaimTypes.UserId, userId.Value.ToString()));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(JwtClaimTypes.Role, role));
        }

        ApplyUser(controller, claims);
    }

    public static void SetMalformedUserId(ControllerBase controller, string rawUserId)
    {
        ApplyUser(controller, [new Claim(JwtClaimTypes.UserId, rawUserId)]);
    }

    private static void ApplyUser(ControllerBase controller, IEnumerable<Claim> claims)
    {
        var identity = new ClaimsIdentity(
            claims,
            authenticationType: "Test",
            nameType: ClaimTypes.Name,
            roleType: JwtClaimTypes.Role);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity),
            },
        };
    }
}
