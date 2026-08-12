using Asp.Versioning;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Administration.Application.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Public)]
[Tags(ApiTags.Administration)]
[AllowAnonymous]
[Route("api/settings/public")]
[Route("api/v{version:apiVersion}/settings/public")]
public sealed class PublicSystemSettingsController : ControllerBase
{
    private readonly IPublicSystemSettingQueries _queries;

    public PublicSystemSettingsController(IPublicSystemSettingQueries queries)
    {
        _queries = queries;
    }

    [HttpGet]
    [OpenApiOperationId("PublicSystemSettings_List")]
    [OpenApiSummary("List public settings", "Returns publicly visible system settings.")]
    [ProducesResponseType(typeof(IReadOnlyList<PublicSystemSettingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PublicSystemSettingDto>>> List(
        CancellationToken cancellationToken)
    {
        return Ok(await _queries.GetPublicAsync(cancellationToken));
    }
}
