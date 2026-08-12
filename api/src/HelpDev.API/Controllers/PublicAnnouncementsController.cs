using Asp.Versioning;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Administration.Application.Announcements;
using HelpDev.SharedKernel.Time;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Public)]
[Tags(ApiTags.Administration)]
[AllowAnonymous]
[Route("api/announcements/active")]
[Route("api/v{version:apiVersion}/announcements/active")]
public sealed class PublicAnnouncementsController : ControllerBase
{
    private readonly IAnnouncementQueries _queries;
    private readonly IDateTimeProvider _clock;

    public PublicAnnouncementsController(IAnnouncementQueries queries, IDateTimeProvider clock)
    {
        _queries = queries;
        _clock = clock;
    }

    [HttpGet]
    [OpenApiOperationId("PublicAnnouncements_List")]
    [OpenApiSummary("List active announcements", "Returns currently active public announcements.")]
    [ProducesResponseType(typeof(IReadOnlyList<ActiveAnnouncementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ActiveAnnouncementDto>>> List(
        CancellationToken cancellationToken)
    {
        return Ok(await _queries.GetActiveAsync(_clock.UtcNow, cancellationToken));
    }
}
