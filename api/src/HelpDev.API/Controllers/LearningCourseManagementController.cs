using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Domain.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Authenticated)]
[Tags(ApiTags.Learning)]
[Route("api/learning/manage/courses")]
[Route("api/v{version:apiVersion}/learning/manage/courses")]
[Authorize(Policy = AuthorizationPolicies.WriterOrAdmin)]
public sealed class LearningCourseManagementController : ControllerBase
{
    private readonly ICourseService _courseService;

    public LearningCourseManagementController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    [OpenApiOperationId("LearningCourseManagement_List")]
    [OpenApiSummary("List courses", "Lists courses visible to the authenticated writer or admin.")]
    [ProducesResponseType(typeof(IReadOnlyList<CourseListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<CourseListItemDto>>> List(
        [FromQuery] CourseStatus? status,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var items = await _courseService.ListAsync(actor, status, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [OpenApiOperationId("LearningCourseManagement_GetById")]
    [OpenApiSummary("Get course by ID", "Returns a course by identifier for management.")]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseDetailDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var course = await _courseService.GetByIdAsync(actor, id, cancellationToken);
        return Ok(course);
    }

    [HttpPost]
    [OpenApiOperationId("LearningCourseManagement_Create")]
    [OpenApiSummary("Create course", "Creates a new draft course.")]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CourseDetailDto>> Create(
        [FromBody] CreateCourseRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var course = await _courseService.CreateAsync(actor, request, cancellationToken);
        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = course.Id },
            value: course);
    }

    [HttpPut("{id:guid}")]
    [OpenApiOperationId("LearningCourseManagement_UpdateDetails")]
    [OpenApiSummary("Update course details", "Updates course metadata and details.")]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CourseDetailDto>> UpdateDetails(
        Guid id,
        [FromBody] UpdateCourseRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var course = await _courseService.UpdateDetailsAsync(actor, id, request, cancellationToken);
        return Ok(course);
    }

    [HttpPost("{id:guid}/publish")]
    [OpenApiOperationId("LearningCourseManagement_Publish")]
    [OpenApiSummary("Publish course", "Publishes a course making it publicly visible.")]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CourseDetailDto>> Publish(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var course = await _courseService.PublishAsync(actor, id, cancellationToken);
        return Ok(course);
    }

    [HttpPost("{id:guid}/sections")]
    [OpenApiOperationId("LearningCourseManagement_AddSection")]
    [OpenApiSummary("Add section", "Adds a section to a course.")]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CourseDetailDto>> AddSection(
        Guid id,
        [FromBody] AddSectionRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var course = await _courseService.AddSectionAsync(actor, id, request, cancellationToken);
        return Ok(course);
    }

    [HttpPut("{id:guid}/sections/{sectionId:guid}")]
    [OpenApiOperationId("LearningCourseManagement_RenameSection")]
    [OpenApiSummary("Rename section", "Renames a course section.")]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CourseDetailDto>> RenameSection(
        Guid id,
        Guid sectionId,
        [FromBody] RenameSectionBody body,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(body);

        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var course = await _courseService.RenameSectionAsync(
            actor,
            id,
            new RenameSectionRequest { SectionId = sectionId, Title = body.Title },
            cancellationToken);
        return Ok(course);
    }

    [HttpPut("{id:guid}/sections/{sectionId:guid}/order")]
    [OpenApiOperationId("LearningCourseManagement_ReorderSection")]
    [OpenApiSummary("Reorder section", "Changes the display order of a course section.")]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CourseDetailDto>> ReorderSection(
        Guid id,
        Guid sectionId,
        [FromBody] ReorderBody body,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(body);

        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var course = await _courseService.ReorderSectionAsync(
            actor,
            id,
            new ReorderSectionRequest { SectionId = sectionId, NewOrder = body.NewOrder },
            cancellationToken);
        return Ok(course);
    }

    [HttpPost("{id:guid}/sections/{sectionId:guid}/lessons")]
    [OpenApiOperationId("LearningCourseManagement_AddLesson")]
    [OpenApiSummary("Add lesson", "Adds a lesson to a course section.")]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CourseDetailDto>> AddLesson(
        Guid id,
        Guid sectionId,
        [FromBody] AddLessonBody body,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(body);

        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var course = await _courseService.AddLessonAsync(
            actor,
            id,
            new AddLessonRequest
            {
                SectionId = sectionId,
                Title = body.Title,
                ContentId = body.ContentId,
                VideoUrl = body.VideoUrl,
                DurationMinutes = body.DurationMinutes,
                IsPreview = body.IsPreview,
            },
            cancellationToken);
        return Ok(course);
    }

    [HttpPut("{id:guid}/sections/{sectionId:guid}/lessons/{lessonId:guid}")]
    [OpenApiOperationId("LearningCourseManagement_UpdateLesson")]
    [OpenApiSummary("Update lesson", "Updates a lesson within a course section.")]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CourseDetailDto>> UpdateLesson(
        Guid id,
        Guid sectionId,
        Guid lessonId,
        [FromBody] UpdateLessonBody body,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(body);

        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var course = await _courseService.UpdateLessonAsync(
            actor,
            id,
            new UpdateLessonRequest
            {
                SectionId = sectionId,
                LessonId = lessonId,
                Title = body.Title,
                ContentId = body.ContentId,
                VideoUrl = body.VideoUrl,
                DurationMinutes = body.DurationMinutes,
                IsPreview = body.IsPreview,
            },
            cancellationToken);
        return Ok(course);
    }

    [HttpPut("{id:guid}/sections/{sectionId:guid}/lessons/{lessonId:guid}/order")]
    [OpenApiOperationId("LearningCourseManagement_ReorderLesson")]
    [OpenApiSummary("Reorder lesson", "Changes the display order of a lesson within a section.")]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CourseDetailDto>> ReorderLesson(
        Guid id,
        Guid sectionId,
        Guid lessonId,
        [FromBody] ReorderBody body,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(body);

        if (!TryResolveActor(out var actor, out var unauthorized))
        {
            return unauthorized;
        }

        var course = await _courseService.ReorderLessonAsync(
            actor,
            id,
            new ReorderLessonRequest
            {
                SectionId = sectionId,
                LessonId = lessonId,
                NewOrder = body.NewOrder,
            },
            cancellationToken);
        return Ok(course);
    }

    private bool TryResolveActor(
        out CourseManagementActor actor,
        out ActionResult unauthorized)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            actor = null!;
            unauthorized = Unauthorized();
            return false;
        }

        actor = new CourseManagementActor(
            userId.Value,
            canManageAllCourses: User.IsInRole(AppRoles.Admin));
        unauthorized = null!;
        return true;
    }
}
