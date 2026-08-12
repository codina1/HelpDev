using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Administration.Application.Announcements;
using HelpDev.Modules.Identity.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Admin)]
[Tags(ApiTags.Administration)]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/announcements")]
[Route("api/v{version:apiVersion}/admin/announcements")]
public sealed class AdministrationAnnouncementsController : ControllerBase
{
    private readonly IAnnouncementService _service;

    public AdministrationAnnouncementsController(IAnnouncementService service)
    {
        _service = service;
    }

    [HttpGet]
    [OpenApiOperationId("AdministrationAnnouncements_List")]
    [OpenApiSummary("List announcements", "Returns a paginated list of announcements.")]
    [ProducesResponseType(typeof(AnnouncementPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AnnouncementPageDto>> List(
        [FromQuery] string? status,
        [FromQuery] string? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = AnnouncementPaging.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetPageAsync(
            new AnnouncementFilter(status, type, page, pageSize),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [OpenApiOperationId("AdministrationAnnouncements_GetById")]
    [OpenApiSummary("Get announcement", "Returns an announcement by identifier.")]
    [ProducesResponseType(typeof(AnnouncementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnnouncementDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [OpenApiOperationId("AdministrationAnnouncements_Create")]
    [OpenApiSummary("Create announcement", "Creates a new announcement draft.")]
    [ProducesResponseType(typeof(AnnouncementDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AnnouncementDto>> Create(
        [FromBody] CreateAnnouncementRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(request, User.GetUserId(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [OpenApiOperationId("AdministrationAnnouncements_Update")]
    [OpenApiSummary("Update announcement", "Updates an announcement's content.")]
    [ProducesResponseType(typeof(AnnouncementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnnouncementDto>> Update(
        Guid id,
        [FromBody] UpdateAnnouncementRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.UpdateAsync(id, request, User.GetUserId(), cancellationToken));
    }

    [HttpPost("{id:guid}/publish")]
    [OpenApiOperationId("AdministrationAnnouncements_Publish")]
    [OpenApiSummary("Publish announcement", "Publishes an announcement making it active.")]
    [ProducesResponseType(typeof(AnnouncementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AnnouncementDto>> Publish(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.PublishAsync(id, User.GetUserId(), cancellationToken));
    }

    [HttpPost("{id:guid}/archive")]
    [OpenApiOperationId("AdministrationAnnouncements_Archive")]
    [OpenApiSummary("Archive announcement", "Archives a published announcement.")]
    [ProducesResponseType(typeof(AnnouncementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AnnouncementDto>> Archive(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.ArchiveAsync(id, User.GetUserId(), cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [OpenApiOperationId("AdministrationAnnouncements_Delete")]
    [OpenApiSummary("Delete announcement", "Deletes an announcement.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, User.GetUserId(), cancellationToken);
        return NoContent();
    }
}
