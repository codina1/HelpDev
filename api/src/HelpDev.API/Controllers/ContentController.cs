using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Identity.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Public)]
[Tags(ApiTags.Content)]
[Route("api/content")]
[Route("api/v{version:apiVersion}/content")]
public sealed class ContentController : ControllerBase
{
    private readonly IContentService _contentService;

    public ContentController(IContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpGet]
    [OpenApiOperationId("Content_ListPublished")]
    [OpenApiSummary("List published content", "Returns all published content items.")]
    [ProducesResponseType(typeof(IReadOnlyList<ContentListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ContentListItemDto>>> List(
        CancellationToken cancellationToken)
    {
        var items = await _contentService.ListPublishedAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{slug}")]
    [OpenApiOperationId("Content_GetBySlug")]
    [OpenApiSummary("Get content by slug", "Returns a published content item by its slug.")]
    [ProducesResponseType(typeof(ContentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentDetailDto>> GetBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        try
        {
            var content = await _contentService.GetPublishedBySlugAsync(slug, User.GetUserId(), cancellationToken);
            return Ok(content);
        }
        catch (ContentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.WriterOrAdmin)]
    [ApiAudience(ApiAudiences.Authenticated)]
    [OpenApiOperationId("Content_Create")]
    [OpenApiSummary("Create content", "Creates a new content item. Requires Writer or Admin role.")]
    [ProducesResponseType(typeof(ContentDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ContentDetailDto>> Create(
        [FromBody] CreateContentRequest request,
        CancellationToken cancellationToken)
    {
        var authorId = User.GetUserId();
        if (authorId is null)
        {
            return Unauthorized();
        }

        try
        {
            var content = await _contentService.CreateAsync(authorId.Value, request, cancellationToken);
            return CreatedAtAction(nameof(GetBySlug), new { slug = content.Slug }, content);
        }
        catch (ContentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
