using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Learning.Application.Personalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Authenticated)]
[Tags(ApiTags.Learning)]
[Authorize(Policy = AuthorizationPolicies.Authenticated)]
[Route("api/me")]
[Route("api/v{version:apiVersion}/me")]
public sealed class LearningPersonalizationMeController : ControllerBase
{
    private readonly ILearningProfileService _profileService;
    private readonly ILearningRecommendationService _recommendationService;
    private readonly ILearningRoadmapService _roadmapService;

    public LearningPersonalizationMeController(
        ILearningProfileService profileService,
        ILearningRecommendationService recommendationService,
        ILearningRoadmapService roadmapService)
    {
        _profileService = profileService;
        _recommendationService = recommendationService;
        _roadmapService = roadmapService;
    }

    [HttpGet("learning-profile")]
    [OpenApiOperationId("LearningMe_GetProfile")]
    [OpenApiSummary("Get learning profile", "Returns the authenticated user's learning profile and preferences.")]
    [ProducesResponseType(typeof(LearningProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public Task<LearningProfileDto> GetProfile(CancellationToken cancellationToken) =>
        _profileService.GetAsync(RequireUserId(), cancellationToken);

    [HttpPut("learning-profile")]
    [OpenApiOperationId("LearningMe_UpsertProfile")]
    [OpenApiSummary("Update learning profile", "User-controlled update. AI never overwrites this profile.")]
    [ProducesResponseType(typeof(LearningProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public Task<LearningProfileDto> UpsertProfile(
        [FromBody] UpdateLearningProfileRequest request,
        CancellationToken cancellationToken) =>
        _profileService.UpsertAsync(RequireUserId(), request, cancellationToken);

    [HttpGet("recommendations")]
    [OpenApiOperationId("LearningMe_GetRecommendations")]
    [OpenApiSummary("Get learning recommendations", "Profile + signals + HelpDev knowledge. Suggestion only.")]
    [ProducesResponseType(typeof(LearningRecommendationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public Task<LearningRecommendationDto> GetRecommendations(CancellationToken cancellationToken) =>
        _recommendationService.GetRecommendationsAsync(RequireUserId(), cancellationToken);

    [HttpGet("roadmap")]
    [OpenApiOperationId("LearningMe_GetRoadmap")]
    [OpenApiSummary("Get personal learning roadmap", "Returns the current suggested or approved roadmap.")]
    [ProducesResponseType(typeof(LearningRoadmapDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LearningRoadmapDto>> GetRoadmap(CancellationToken cancellationToken)
    {
        var roadmap = await _roadmapService.GetAsync(RequireUserId(), cancellationToken);
        return roadmap is null ? NoContent() : Ok(roadmap);
    }

    [HttpPost("roadmap/generate")]
    [OpenApiOperationId("LearningMe_GenerateRoadmap")]
    [OpenApiSummary("Generate learning roadmap", "AI suggests steps. Does not enroll or change progress.")]
    [ProducesResponseType(typeof(LearningRoadmapDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public Task<LearningRoadmapDto> GenerateRoadmap(
        [FromBody] GenerateLearningRoadmapRequest? request,
        CancellationToken cancellationToken) =>
        _roadmapService.GenerateAsync(
            RequireUserId(),
            request ?? new GenerateLearningRoadmapRequest(null),
            cancellationToken);

    [HttpPost("roadmap/approve")]
    [OpenApiOperationId("LearningMe_ApproveRoadmap")]
    [OpenApiSummary("Approve learning roadmap", "Explicit user approval of the suggested roadmap.")]
    [ProducesResponseType(typeof(LearningRoadmapDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public Task<LearningRoadmapDto> ApproveRoadmap(CancellationToken cancellationToken) =>
        _roadmapService.ApproveAsync(RequireUserId(), cancellationToken);

    private Guid RequireUserId()
    {
        var userId = User.GetUserId();
        if (userId is null || userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Authenticated user id is required.");
        }

        return userId.Value;
    }
}

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Admin)]
[Tags(ApiTags.Learning)]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/learning/personalization")]
[Route("api/v{version:apiVersion}/admin/learning/personalization")]
public sealed class LearningPersonalizationAdminController : ControllerBase
{
    private readonly ILearningPersonalizationAdminQueries _queries;

    public LearningPersonalizationAdminController(ILearningPersonalizationAdminQueries queries)
    {
        _queries = queries;
    }

    [HttpGet]
    [OpenApiOperationId("LearningAdmin_GetPersonalizationSummary")]
    [OpenApiSummary("Learning personalization summary", "Aggregate counts only. No private profile content.")]
    [ProducesResponseType(typeof(LearningPersonalizationAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<LearningPersonalizationAdminDto> GetSummary(CancellationToken cancellationToken) =>
        _queries.GetSummaryAsync(cancellationToken);
}
