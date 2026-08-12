using Asp.Versioning;
using HelpDev.API.Contracts;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Analytics.Application.ContentAnalytics;
using HelpDev.Modules.Analytics.Application.Queries;
using HelpDev.Modules.Analytics.Domain;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.SharedKernel.Time;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDev.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Admin)]
[Tags(ApiTags.Analytics)]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/analytics")]
[Route("api/v{version:apiVersion}/admin/analytics")]
public sealed class AnalyticsAdminController : ControllerBase
{
    private readonly IAnalyticsOverviewQueries _overviewQueries;
    private readonly IAnalyticsTimeSeriesQueries _timeSeriesQueries;
    private readonly IAnalyticsTopItemsQueries _topItemsQueries;
    private readonly ISearchAnalyticsQueries _searchAnalyticsQueries;
    private readonly IToolboxAnalyticsQueries _toolboxAnalyticsQueries;
    private readonly IPromptLabAnalyticsQueries _promptLabAnalyticsQueries;
    private readonly IContentAnalyticsQueries _contentAnalyticsQueries;
    private readonly IDateTimeProvider _clock;
    private readonly AnalyticsOptions _options;

    public AnalyticsAdminController(
        IAnalyticsOverviewQueries overviewQueries,
        IAnalyticsTimeSeriesQueries timeSeriesQueries,
        IAnalyticsTopItemsQueries topItemsQueries,
        ISearchAnalyticsQueries searchAnalyticsQueries,
        IToolboxAnalyticsQueries toolboxAnalyticsQueries,
        IPromptLabAnalyticsQueries promptLabAnalyticsQueries,
        IContentAnalyticsQueries contentAnalyticsQueries,
        IDateTimeProvider clock,
        Microsoft.Extensions.Options.IOptions<AnalyticsOptions> options)
    {
        _overviewQueries = overviewQueries;
        _timeSeriesQueries = timeSeriesQueries;
        _topItemsQueries = topItemsQueries;
        _searchAnalyticsQueries = searchAnalyticsQueries;
        _toolboxAnalyticsQueries = toolboxAnalyticsQueries;
        _promptLabAnalyticsQueries = promptLabAnalyticsQueries;
        _contentAnalyticsQueries = contentAnalyticsQueries;
        _clock = clock;
        _options = options.Value;
    }

    [HttpGet("overview")]
    [OpenApiOperationId("AnalyticsAdmin_GetOverview")]
    [OpenApiSummary("Get analytics overview", "Returns high-level analytics metrics for a date range.")]
    [ProducesResponseType(typeof(AnalyticsOverviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<AnalyticsOverviewDto> GetOverview(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var range = ResolveRange(from, to);
        return _overviewQueries.GetOverviewAsync(range, cancellationToken);
    }

    [HttpGet("content")]
    [OpenApiOperationId("AnalyticsAdmin_GetContentOverview")]
    [OpenApiSummary("Get content analytics overview", "Real content views/created/published from analytics_daily_metrics. No invented traffic.")]
    [ProducesResponseType(typeof(ContentAnalyticsOverviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<ContentAnalyticsOverviewDto> GetContentOverview(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken) =>
        _contentAnalyticsQueries.GetContentOverviewAsync(ResolveRange(from, to), cancellationToken);

    [HttpGet("content/{id:guid}")]
    [OpenApiOperationId("AnalyticsAdmin_GetContentPerformance")]
    [OpenApiSummary("Get content performance", "Per-content metrics and health from stored analytics + content facts.")]
    [ProducesResponseType(typeof(ContentItemAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentItemAnalyticsDto>> GetContentPerformance(
        Guid id,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var range = ResolveRange(from, to);
        var performance = await _contentAnalyticsQueries.GetContentPerformanceAsync(id, range, cancellationToken);
        if (performance is null)
        {
            return NotFound();
        }

        var health = await _contentAnalyticsQueries.GetContentHealthByIdAsync(id, range, cancellationToken);
        return Ok(new ContentItemAnalyticsDto(performance, health));
    }

    [HttpGet("top-content")]
    [OpenApiOperationId("AnalyticsAdmin_GetTopContentAnalytics")]
    [OpenApiSummary("Get top content by views", "Subject-scoped content.views only (no double-counting).")]
    [ProducesResponseType(typeof(IReadOnlyList<ContentPerformanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<IReadOnlyList<ContentPerformanceDto>> GetTopContentAnalytics(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int? limit,
        CancellationToken cancellationToken) =>
        _contentAnalyticsQueries.GetTopContentAsync(ResolveRange(from, to), ResolveLimit(limit), cancellationToken);

    [HttpGet("content-health")]
    [OpenApiOperationId("AnalyticsAdmin_GetContentHealth")]
    [OpenApiSummary("Get content health indicators", "Transparent reasons only — no fake SEO/ranking scores.")]
    [ProducesResponseType(typeof(IReadOnlyList<ContentHealthIndicatorDto>), StatusCodes.Status200OK)]
    public Task<IReadOnlyList<ContentHealthIndicatorDto>> GetContentHealth(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int? limit,
        CancellationToken cancellationToken) =>
        _contentAnalyticsQueries.GetContentHealthAsync(ResolveRange(from, to), ResolveLimit(limit), cancellationToken);

    [HttpGet("time-series")]
    [OpenApiOperationId("AnalyticsAdmin_GetTimeSeries")]
    [OpenApiSummary("Get analytics time series", "Returns time-series data for a metric and date range.")]
    [ProducesResponseType(typeof(AnalyticsTimeSeriesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<AnalyticsTimeSeriesDto> GetTimeSeries(
        [FromQuery] string metric,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] Guid? subjectId,
        [FromQuery] string? dimensionKey,
        [FromQuery] string? dimensionValue,
        CancellationToken cancellationToken)
    {
        var range = ResolveRange(from, to);
        return _timeSeriesQueries.GetTimeSeriesAsync(
            new AnalyticsTimeSeriesRequest(metric, range.FromUtc, range.ToUtc, subjectId, dimensionKey, dimensionValue),
            cancellationToken);
    }

    [HttpGet("top/content")]
    [OpenApiOperationId("AnalyticsAdmin_GetTopContent")]
    [OpenApiSummary("Get top content", "Returns the most viewed content items for a date range.")]
    [ProducesResponseType(typeof(IReadOnlyList<AnalyticsTopItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<IReadOnlyList<AnalyticsTopItemDto>> GetTopContent(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int? limit,
        CancellationToken cancellationToken) =>
        _topItemsQueries.GetTopContentAsync(ResolveRange(from, to), ResolveLimit(limit), cancellationToken);

    [HttpGet("top/courses")]
    [OpenApiOperationId("AnalyticsAdmin_GetTopCourses")]
    [OpenApiSummary("Get top courses", "Returns the most viewed courses for a date range.")]
    [ProducesResponseType(typeof(IReadOnlyList<AnalyticsTopItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<IReadOnlyList<AnalyticsTopItemDto>> GetTopCourses(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int? limit,
        CancellationToken cancellationToken) =>
        _topItemsQueries.GetTopCoursesAsync(ResolveRange(from, to), ResolveLimit(limit), cancellationToken);

    [HttpGet("top/tools")]
    [OpenApiOperationId("AnalyticsAdmin_GetTopTools")]
    [OpenApiSummary("Get top tools", "Returns the most used toolbox tools for a date range.")]
    [ProducesResponseType(typeof(IReadOnlyList<AnalyticsTopItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<IReadOnlyList<AnalyticsTopItemDto>> GetTopTools(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int? limit,
        CancellationToken cancellationToken) =>
        _topItemsQueries.GetTopToolsAsync(ResolveRange(from, to), ResolveLimit(limit), cancellationToken);

    [HttpGet("top/prompts")]
    [OpenApiOperationId("AnalyticsAdmin_GetTopPrompts")]
    [OpenApiSummary("Get top prompts", "Returns the most used prompts for a date range.")]
    [ProducesResponseType(typeof(IReadOnlyList<AnalyticsTopItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<IReadOnlyList<AnalyticsTopItemDto>> GetTopPrompts(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int? limit,
        CancellationToken cancellationToken) =>
        _topItemsQueries.GetTopPromptsAsync(ResolveRange(from, to), ResolveLimit(limit), cancellationToken);

    [HttpGet("search")]
    [OpenApiOperationId("AnalyticsAdmin_GetSearchAnalytics")]
    [OpenApiSummary("Get search analytics", "Returns search usage analytics for a date range.")]
    [ProducesResponseType(typeof(SearchAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<SearchAnalyticsDto> GetSearchAnalytics(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken) =>
        _searchAnalyticsQueries.GetAsync(ResolveRange(from, to), cancellationToken);

    [HttpGet("toolbox")]
    [OpenApiOperationId("AnalyticsAdmin_GetToolboxAnalytics")]
    [OpenApiSummary("Get toolbox analytics", "Returns toolbox execution analytics for a date range.")]
    [ProducesResponseType(typeof(ExecutionAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<ExecutionAnalyticsDto> GetToolboxAnalytics(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken) =>
        _toolboxAnalyticsQueries.GetAsync(ResolveRange(from, to), cancellationToken);

    [HttpGet("prompt-lab")]
    [OpenApiOperationId("AnalyticsAdmin_GetPromptLabAnalytics")]
    [OpenApiSummary("Get PromptLab analytics", "Returns PromptLab render analytics for a date range.")]
    [ProducesResponseType(typeof(ExecutionAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<ExecutionAnalyticsDto> GetPromptLabAnalytics(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken) =>
        _promptLabAnalyticsQueries.GetAsync(ResolveRange(from, to), cancellationToken);

    private AnalyticsDateRange ResolveRange(DateOnly? from, DateOnly? to)
    {
        var today = DateOnly.FromDateTime(_clock.UtcNow);
        return new AnalyticsDateRange(from ?? today.AddDays(-29), to ?? today);
    }

    private int ResolveLimit(int? limit) => limit ?? _options.DefaultTopLimit;
}

public sealed record ContentItemAnalyticsDto(
    ContentPerformanceDto Performance,
    ContentHealthIndicatorDto? Health);
