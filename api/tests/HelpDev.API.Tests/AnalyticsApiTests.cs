using System.Reflection;
using HelpDev.API.Controllers;
using HelpDev.API.Filters;
using HelpDev.API.Tests.Fakes;
using HelpDev.Modules.Analytics.Application;
using HelpDev.Modules.Analytics.Application.ContentAnalytics;
using HelpDev.Modules.Analytics.Application.Queries;
using HelpDev.Modules.Analytics.Domain;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.SharedKernel.Time;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace HelpDev.API.Tests;

public sealed class AnalyticsApiTests
{
    [Fact]
    public void Analytics_admin_controller_requires_AdminOnly_policy()
    {
        var attribute = Assert.Single(
            typeof(AnalyticsAdminController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>());

        Assert.Equal(AuthorizationPolicies.AdminOnly, attribute.Policy);
    }

    [Fact]
    public void Analytics_admin_controller_depends_on_query_abstractions_not_repositories()
    {
        var parameters = typeof(AnalyticsAdminController).GetConstructors().Single().GetParameters();

        Assert.Contains(parameters, p => p.ParameterType == typeof(IAnalyticsOverviewQueries));
        Assert.Contains(parameters, p => p.ParameterType == typeof(IContentAnalyticsQueries));
        Assert.DoesNotContain(
            parameters,
            p => p.ParameterType.Name.Contains("Repository", StringComparison.Ordinal)
                || p.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal));
    }

    [Fact]
    public void Content_analytics_routes_exist_on_admin_controller()
    {
        var methods = typeof(AnalyticsAdminController).GetMethods(BindingFlags.Instance | BindingFlags.Public);
        Assert.Contains(methods, m => m.Name == nameof(AnalyticsAdminController.GetContentOverview));
        Assert.Contains(methods, m => m.Name == nameof(AnalyticsAdminController.GetContentPerformance));
        Assert.Contains(methods, m => m.Name == nameof(AnalyticsAdminController.GetTopContentAnalytics));
    }

    [Fact]
    public void Content_analytics_dtos_expose_no_score_or_fake_growth()
    {
        foreach (var type in new[]
                 {
                     typeof(ContentAnalyticsOverviewDto),
                     typeof(ContentPerformanceDto),
                     typeof(ContentHealthIndicatorDto),
                     typeof(ContentMetricDto),
                 })
        {
            var names = type.GetProperties().Select(p => p.Name).ToArray();
            Assert.DoesNotContain(names, n => n.Contains("Score", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(names, n => n.Contains("Rank", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(names, n => n.Contains("Prediction", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public async Task GetOverview_returns_ok_with_default_range()
    {
        var (controller, overviewQueries) = CreateController();

        var result = await controller.GetOverview(from: null, to: null, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1, overviewQueries.CallCount);
    }

    [Fact]
    public async Task GetOverview_with_explicit_range_passes_range_to_queries()
    {
        var (controller, overviewQueries) = CreateController();
        var from = new DateOnly(2026, 6, 1);
        var to = new DateOnly(2026, 6, 30);

        await controller.GetOverview(from, to, CancellationToken.None);

        Assert.Equal(from, overviewQueries.LastRange?.FromUtc);
        Assert.Equal(to, overviewQueries.LastRange?.ToUtc);
    }

    [Fact]
    public void Exception_filter_maps_date_range_invalid_to_400()
    {
        AssertFilterMapsTo(AnalyticsApplicationErrorCodes.DateRangeInvalid, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void Exception_filter_maps_date_range_too_large_to_400()
    {
        AssertFilterMapsTo(AnalyticsApplicationErrorCodes.DateRangeTooLarge, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void Exception_filter_maps_event_type_unsupported_to_400()
    {
        AssertFilterMapsTo(AnalyticsApplicationErrorCodes.EventTypeUnsupported, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void Exception_filter_maps_event_dimensions_invalid_to_400()
    {
        AssertFilterMapsTo(AnalyticsApplicationErrorCodes.EventDimensionsInvalid, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void Exception_filter_ignores_non_analytics_exceptions()
    {
        var filter = new AnalyticsExceptionFilter();
        var context = MakeContext(new InvalidOperationException("boom"));

        filter.OnException(context);

        Assert.Null(context.Result);
        Assert.False(context.ExceptionHandled);
    }

    private static void AssertFilterMapsTo(string code, int expectedStatus)
    {
        var filter = new AnalyticsExceptionFilter();
        var context = MakeContext(new AnalyticsException("failed", code));

        filter.OnException(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(expectedStatus, result.StatusCode);
        Assert.True(context.ExceptionHandled);
    }

    private static ExceptionContext MakeContext(Exception exception)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());
        return new ExceptionContext(actionContext, []) { Exception = exception };
    }

    private static (AnalyticsAdminController Controller, FakeAnalyticsOverviewQueries OverviewQueries) CreateController()
    {
        var overviewQueries = new FakeAnalyticsOverviewQueries();
        var controller = new AnalyticsAdminController(
            overviewQueries,
            new FakeAnalyticsTimeSeriesQueries(),
            new FakeAnalyticsTopItemsQueries(),
            new FakeSearchAnalyticsQueries(),
            new FakeToolboxAnalyticsQueries(),
            new FakePromptLabAnalyticsQueries(),
            new FakeContentAnalyticsQueries(),
            new FakeDateTimeProvider(new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc)),
            Options.Create(new AnalyticsOptions { DefaultTopLimit = 10, MaxQueryRangeDays = 366 }));
        return (controller, overviewQueries);
    }

    private sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public FakeDateTimeProvider(DateTime utcNow) => UtcNow = utcNow;
        public DateTime UtcNow { get; }
    }

    private sealed class FakeAnalyticsOverviewQueries : IAnalyticsOverviewQueries
    {
        public int CallCount { get; private set; }
        public AnalyticsDateRange? LastRange { get; private set; }

        public Task<AnalyticsOverviewDto> GetOverviewAsync(AnalyticsDateRange range, CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastRange = range;
            return Task.FromResult(new AnalyticsOverviewDto(
                range,
                new AnalyticsUsersOverviewDto(0, 0, 0),
                new AnalyticsContentOverviewDto(0, 0, 0),
                new AnalyticsLearningOverviewDto(0, 0, 0, 0),
                new AnalyticsSearchOverviewDto(0, 0, 0),
                new AnalyticsToolboxOverviewDto(0, 0, 0, 0, 0),
                new AnalyticsPromptLabOverviewDto(0, 0, 0, 0, 0)));
        }
    }

    private sealed class FakeAnalyticsTimeSeriesQueries : IAnalyticsTimeSeriesQueries
    {
        public Task<AnalyticsTimeSeriesDto> GetTimeSeriesAsync(AnalyticsTimeSeriesRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AnalyticsTimeSeriesDto(request.MetricKey, request.FromUtc, request.ToUtc, []));
    }

    private sealed class FakeAnalyticsTopItemsQueries : IAnalyticsTopItemsQueries
    {
        public Task<IReadOnlyList<AnalyticsTopItemDto>> GetTopContentAsync(AnalyticsDateRange range, int limit, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AnalyticsTopItemDto>>([]);

        public Task<IReadOnlyList<AnalyticsTopItemDto>> GetTopCoursesAsync(AnalyticsDateRange range, int limit, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AnalyticsTopItemDto>>([]);

        public Task<IReadOnlyList<AnalyticsTopItemDto>> GetTopToolsAsync(AnalyticsDateRange range, int limit, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AnalyticsTopItemDto>>([]);

        public Task<IReadOnlyList<AnalyticsTopItemDto>> GetTopPromptsAsync(AnalyticsDateRange range, int limit, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AnalyticsTopItemDto>>([]);
    }

    private sealed class FakeSearchAnalyticsQueries : ISearchAnalyticsQueries
    {
        public Task<SearchAnalyticsDto> GetAsync(AnalyticsDateRange range, CancellationToken cancellationToken = default) =>
            Task.FromResult(new SearchAnalyticsDto(0, 0, 0, 0, 0, new Dictionary<string, long>(), []));
    }

    private sealed class FakeToolboxAnalyticsQueries : IToolboxAnalyticsQueries
    {
        public Task<ExecutionAnalyticsDto> GetAsync(AnalyticsDateRange range, CancellationToken cancellationToken = default) =>
            Task.FromResult(new ExecutionAnalyticsDto(0, 0, 0, 0, 0, [], new Dictionary<string, long>()));
    }

    private sealed class FakePromptLabAnalyticsQueries : IPromptLabAnalyticsQueries
    {
        public Task<ExecutionAnalyticsDto> GetAsync(AnalyticsDateRange range, CancellationToken cancellationToken = default) =>
            Task.FromResult(new ExecutionAnalyticsDto(0, 0, 0, 0, 0, [], new Dictionary<string, long>()));
    }

    private sealed class FakeContentAnalyticsQueries : IContentAnalyticsQueries
    {
        public Task<ContentAnalyticsOverviewDto> GetContentOverviewAsync(
            AnalyticsDateRange range,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new ContentAnalyticsOverviewDto(range, 0, 0, 0, 0, []));

        public Task<IReadOnlyList<ContentPerformanceDto>> GetTopContentAsync(
            AnalyticsDateRange range,
            int limit,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ContentPerformanceDto>>([]);

        public Task<ContentPerformanceDto?> GetContentPerformanceAsync(
            Guid contentId,
            AnalyticsDateRange range,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<ContentPerformanceDto?>(null);

        public Task<IReadOnlyList<ContentHealthIndicatorDto>> GetContentHealthAsync(
            AnalyticsDateRange range,
            int limit,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ContentHealthIndicatorDto>>([]);

        public Task<ContentHealthIndicatorDto?> GetContentHealthByIdAsync(
            Guid contentId,
            AnalyticsDateRange range,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<ContentHealthIndicatorDto?>(null);
    }
}
