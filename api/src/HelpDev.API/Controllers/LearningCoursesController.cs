using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Application.Courses.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Public)]
[Tags(ApiTags.Learning)]
[Route("api/learning/courses")]
[Route("api/v{version:apiVersion}/learning/courses")]
public sealed class LearningCoursesController : ControllerBase
{
    private readonly IPublicCourseQueries _publicCourseQueries;

    public LearningCoursesController(IPublicCourseQueries publicCourseQueries)
    {
        _publicCourseQueries = publicCourseQueries;
    }

    [HttpGet]
    [OpenApiOperationId("LearningCourses_List")]
    [OpenApiSummary("List published courses", "Returns all published learning courses.")]
    [ProducesResponseType(typeof(IReadOnlyList<CourseListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CourseListItemDto>>> List(
        CancellationToken cancellationToken)
    {
        var items = await _publicCourseQueries.ListPublishedAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [OpenApiOperationId("LearningCourses_GetById")]
    [OpenApiSummary("Get course by ID", "Returns a published course by its identifier.")]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseDetailDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var course = await _publicCourseQueries.GetPublishedByIdAsync(id, cancellationToken);
        if (course is null)
        {
            throw new CourseException("Course was not found.", CourseErrorCodes.NotFound);
        }

        return Ok(course);
    }

    [HttpGet("by-slug/{slug}")]
    [OpenApiOperationId("LearningCourses_GetBySlug")]
    [OpenApiSummary("Get course by slug", "Returns a published course by its slug.")]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseDetailDto>> GetBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var course = await _publicCourseQueries.GetPublishedBySlugAsync(slug, cancellationToken);
        if (course is null)
        {
            throw new CourseException("Course was not found.", CourseErrorCodes.NotFound);
        }

        return Ok(course);
    }
}
