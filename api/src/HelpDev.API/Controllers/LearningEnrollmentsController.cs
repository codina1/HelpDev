using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Learning.Application.Enrollments;
using HelpDev.Modules.Learning.Application.Enrollments.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

/// <summary>
/// Student enrollment and lesson progress endpoints.
/// User identity always comes from JWT claims; never from route, query, or body.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Authenticated)]
[Tags(ApiTags.Learning)]
[Authorize(Policy = AuthorizationPolicies.Authenticated)]
[Route("api/learning")]
[Route("api/v{version:apiVersion}/learning")]
public sealed class LearningEnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public LearningEnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpPost("courses/{courseId:guid}/enroll")]
    [OpenApiOperationId("LearningEnrollments_Enroll")]
    [OpenApiSummary("Enroll in course", "Enrolls the authenticated user in a published course.")]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EnrollmentDto>> Enroll(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var enrollment = await _enrollmentService.EnrollAsync(
            new EnrollStudentRequest
            {
                CourseId = courseId,
                UserId = userId.Value,
            },
            cancellationToken);

        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { enrollmentId = enrollment.Id },
            value: enrollment);
    }

    [HttpGet("me/enrollments")]
    [OpenApiOperationId("LearningEnrollments_ListMine")]
    [OpenApiSummary("List my enrollments", "Returns all enrollments for the authenticated user.")]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<EnrollmentListItemDto>>> ListMine(
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var items = await _enrollmentService.ListByUserAsync(userId.Value, cancellationToken);
        return Ok(items);
    }

    [HttpGet("me/enrollments/{enrollmentId:guid}")]
    [OpenApiOperationId("LearningEnrollments_GetById")]
    [OpenApiSummary("Get enrollment by ID", "Returns a single enrollment for the authenticated user.")]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnrollmentDto>> GetById(
        Guid enrollmentId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var enrollment = await _enrollmentService.GetByIdAsync(
            enrollmentId,
            userId.Value,
            cancellationToken);
        return Ok(enrollment);
    }

    [HttpGet("me/enrollments/by-course/{courseId:guid}")]
    [OpenApiOperationId("LearningEnrollments_GetByCourse")]
    [OpenApiSummary("Get enrollment by course", "Returns the authenticated user's enrollment for a course.")]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnrollmentDto>> GetByCourse(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var enrollment = await _enrollmentService.GetByCourseAndUserAsync(
            courseId,
            userId.Value,
            cancellationToken);
        return Ok(enrollment);
    }

    [HttpPost("courses/{courseId:guid}/lessons/{lessonId:guid}/start")]
    [OpenApiOperationId("LearningEnrollments_StartLesson")]
    [OpenApiSummary("Start lesson", "Marks a lesson as started for the authenticated user.")]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EnrollmentDto>> StartLesson(
        Guid courseId,
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var enrollment = await _enrollmentService.StartLessonAsync(
            new StartLessonRequest
            {
                CourseId = courseId,
                UserId = userId.Value,
                LessonId = lessonId,
            },
            cancellationToken);

        return Ok(enrollment);
    }

    [HttpPost("courses/{courseId:guid}/lessons/{lessonId:guid}/complete")]
    [OpenApiOperationId("LearningEnrollments_CompleteLesson")]
    [OpenApiSummary("Complete lesson", "Marks a lesson as completed for the authenticated user.")]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EnrollmentDto>> CompleteLesson(
        Guid courseId,
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var enrollment = await _enrollmentService.CompleteLessonAsync(
            new CompleteLessonRequest
            {
                CourseId = courseId,
                UserId = userId.Value,
                LessonId = lessonId,
            },
            cancellationToken);

        return Ok(enrollment);
    }
}
