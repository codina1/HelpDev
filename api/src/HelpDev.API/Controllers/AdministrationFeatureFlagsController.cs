using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Administration.Application.FeatureFlags;
using HelpDev.Modules.Identity.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Admin)]
[Tags(ApiTags.Administration)]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/features")]
[Route("api/v{version:apiVersion}/admin/features")]
public sealed class AdministrationFeatureFlagsController : ControllerBase
{
    private readonly IFeatureFlagService _service;

    public AdministrationFeatureFlagsController(IFeatureFlagService service)
    {
        _service = service;
    }

    [HttpGet]
    [OpenApiOperationId("AdministrationFeatureFlags_List")]
    [OpenApiSummary("List feature flags", "Returns all feature flags.")]
    [ProducesResponseType(typeof(IReadOnlyList<FeatureFlagDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<FeatureFlagDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAllAsync(cancellationToken));
    }

    [HttpGet("{key}")]
    [OpenApiOperationId("AdministrationFeatureFlags_GetByKey")]
    [OpenApiSummary("Get feature flag", "Returns a feature flag by key.")]
    [ProducesResponseType(typeof(FeatureFlagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeatureFlagDto>> GetByKey(string key, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetByKeyAsync(key, cancellationToken));
    }

    [HttpPost]
    [OpenApiOperationId("AdministrationFeatureFlags_Create")]
    [OpenApiSummary("Create feature flag", "Creates a new feature flag.")]
    [ProducesResponseType(typeof(FeatureFlagDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FeatureFlagDto>> Create(
        [FromBody] CreateFeatureFlagRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(request, User.GetUserId(), cancellationToken);
        return CreatedAtAction(nameof(GetByKey), new { key = created.Key }, created);
    }

    [HttpPut("{key}")]
    [OpenApiOperationId("AdministrationFeatureFlags_Update")]
    [OpenApiSummary("Update feature flag", "Updates a feature flag's metadata.")]
    [ProducesResponseType(typeof(FeatureFlagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeatureFlagDto>> Update(
        string key,
        [FromBody] UpdateFeatureFlagRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.UpdateAsync(key, request, User.GetUserId(), cancellationToken));
    }

    [HttpPut("{key}/state")]
    [OpenApiOperationId("AdministrationFeatureFlags_SetState")]
    [OpenApiSummary("Set feature flag state", "Enables or disables a feature flag.")]
    [ProducesResponseType(typeof(FeatureFlagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeatureFlagDto>> SetState(
        string key,
        [FromBody] SetFeatureFlagStateRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Ok(await _service.SetEnabledAsync(key, request.IsEnabled, User.GetUserId(), cancellationToken));
    }
}
