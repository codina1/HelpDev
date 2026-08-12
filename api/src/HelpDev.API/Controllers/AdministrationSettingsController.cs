using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Administration.Application.Settings;
using HelpDev.Modules.Identity.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Admin)]
[Tags(ApiTags.Administration)]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/settings")]
[Route("api/v{version:apiVersion}/admin/settings")]
public sealed class AdministrationSettingsController : ControllerBase
{
    private readonly ISystemSettingService _service;

    public AdministrationSettingsController(ISystemSettingService service)
    {
        _service = service;
    }

    [HttpGet]
    [OpenApiOperationId("AdministrationSettings_List")]
    [OpenApiSummary("List system settings", "Returns all system settings.")]
    [ProducesResponseType(typeof(IReadOnlyList<SystemSettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<SystemSettingDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAllAsync(cancellationToken));
    }

    [HttpGet("{key}")]
    [OpenApiOperationId("AdministrationSettings_GetByKey")]
    [OpenApiSummary("Get system setting", "Returns a system setting by key.")]
    [ProducesResponseType(typeof(SystemSettingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SystemSettingDto>> GetByKey(string key, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetByKeyAsync(key, cancellationToken));
    }

    [HttpPost]
    [OpenApiOperationId("AdministrationSettings_Create")]
    [OpenApiSummary("Create system setting", "Creates a new system setting.")]
    [ProducesResponseType(typeof(SystemSettingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SystemSettingDto>> Create(
        [FromBody] CreateSystemSettingRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(request, User.GetUserId(), cancellationToken);
        return CreatedAtAction(nameof(GetByKey), new { key = created.Key }, created);
    }

    [HttpPut("{key}")]
    [OpenApiOperationId("AdministrationSettings_Update")]
    [OpenApiSummary("Update system setting", "Updates a system setting's value.")]
    [ProducesResponseType(typeof(SystemSettingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SystemSettingDto>> Update(
        string key,
        [FromBody] UpdateSystemSettingRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.UpdateAsync(key, request, User.GetUserId(), cancellationToken));
    }
}
