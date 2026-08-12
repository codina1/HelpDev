using System.Reflection;
using HelpDev.API.Controllers;
using HelpDev.API.Filters;
using HelpDev.API.Tests.Fakes;
using HelpDev.Modules.Administration.Application;
using HelpDev.Modules.Administration.Application.Announcements;
using HelpDev.Modules.Administration.Application.Dashboard;
using HelpDev.Modules.Administration.Application.FeatureFlags;
using HelpDev.Modules.Administration.Application.Settings;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.SharedApplication.Abstractions.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.API.Tests;

public sealed class AdministrationApiTests
{
    [Theory]
    [InlineData(typeof(AdministrationDashboardController))]
    [InlineData(typeof(AdministrationFeatureFlagsController))]
    [InlineData(typeof(AdministrationSettingsController))]
    [InlineData(typeof(AdministrationAnnouncementsController))]
    public void Admin_controllers_require_AdminOnly(Type controllerType)
    {
        var attribute = Assert.Single(
            controllerType
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>());

        Assert.Equal(AuthorizationPolicies.AdminOnly, attribute.Policy);
        Assert.NotEqual(AuthorizationPolicies.WriterOrAdmin, attribute.Policy);
    }

    [Fact]
    public void Dashboard_controller_depends_only_on_dashboard_queries()
    {
        var parameters = typeof(AdministrationDashboardController).GetConstructors().Single().GetParameters();
        Assert.Single(parameters);
        Assert.Equal(typeof(IAdministrationDashboardQueries), parameters[0].ParameterType);
        Assert.DoesNotContain(
            parameters,
            p => p.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal)
                || p.ParameterType.Name.Contains("Repository", StringComparison.Ordinal)
                || p.ParameterType == typeof(IDomainEventDispatcher));
    }

    [Fact]
    public void Create_request_dtos_do_not_expose_server_owned_fields()
    {
        Assert.Null(typeof(CreateFeatureFlagRequest).GetProperty("Id"));
        Assert.Null(typeof(CreateFeatureFlagRequest).GetProperty("CreatedAtUtc"));
        Assert.Null(typeof(CreateSystemSettingRequest).GetProperty("Id"));
        Assert.Null(typeof(CreateSystemSettingRequest).GetProperty("UpdatedAtUtc"));
        Assert.Null(typeof(CreateAnnouncementRequest).GetProperty("Id"));
        Assert.Null(typeof(CreateAnnouncementRequest).GetProperty("Status"));
        Assert.Null(typeof(CreateAnnouncementRequest).GetProperty("PublishedAtUtc"));
    }

    [Fact]
    public async Task Dashboard_returns_200_and_forwards_cancellation()
    {
        var queries = new FakeDashboardQueries();
        var controller = new AdministrationDashboardController(queries);
        ControllerTestHelper.SetUser(controller, Guid.NewGuid(), AppRoles.Admin);
        using var cts = new CancellationTokenSource();

        var result = await controller.Get(cts.Token);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(cts.Token, queries.LastToken);
    }

    [Fact]
    public async Task Feature_create_returns_201_and_forwards_admin_id()
    {
        var service = new FakeFeatureFlagService();
        var controller = new AdministrationFeatureFlagsController(service);
        var adminId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, adminId, AppRoles.Admin);

        var result = await controller.Create(
            new CreateFeatureFlagRequest("SearchEnabled", true, null),
            CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(adminId, service.LastAdministratorId);
        Assert.NotNull(created.Value);
    }

    [Fact]
    public async Task Settings_and_announcements_forward_operations()
    {
        var settings = new FakeSystemSettingService();
        var announcements = new FakeAnnouncementService();
        var settingsController = new AdministrationSettingsController(settings);
        var announcementsController = new AdministrationAnnouncementsController(announcements);
        var adminId = Guid.NewGuid();
        ControllerTestHelper.SetUser(settingsController, adminId, AppRoles.Admin);
        ControllerTestHelper.SetUser(announcementsController, adminId, AppRoles.Admin);

        await settingsController.Create(
            new CreateSystemSettingRequest("SiteName", "HelpDev", "String", null, true),
            CancellationToken.None);
        await announcementsController.Create(
            new CreateAnnouncementRequest("Title", "Body", "Information", null, null),
            CancellationToken.None);
        await announcementsController.Publish(Guid.NewGuid(), CancellationToken.None);
        await announcementsController.Archive(Guid.NewGuid(), CancellationToken.None);
        await announcementsController.Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(adminId, settings.LastAdministratorId);
        Assert.Equal(adminId, announcements.LastAdministratorId);
        Assert.True(announcements.PublishCalled);
        Assert.True(announcements.ArchiveCalled);
        Assert.True(announcements.DeleteCalled);
    }

    [Fact]
    public async Task Public_settings_and_announcements_are_anonymous_read_only()
    {
        Assert.NotNull(typeof(PublicSystemSettingsController)
            .GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.NotNull(typeof(PublicAnnouncementsController)
            .GetCustomAttribute<AllowAnonymousAttribute>());

        var publicSettings = new FakePublicSettingsQueries();
        var publicAnnouncements = new FakeAnnouncementQueriesForApi();
        var settingsController = new PublicSystemSettingsController(publicSettings);
        var announcementsController = new PublicAnnouncementsController(
            publicAnnouncements,
            new FixedClock(new DateTime(2026, 7, 19, 12, 0, 0, DateTimeKind.Utc)));

        Assert.IsType<OkObjectResult>((await settingsController.List(CancellationToken.None)).Result);
        Assert.IsType<OkObjectResult>((await announcementsController.List(CancellationToken.None)).Result);
        Assert.True(publicSettings.Called);
        Assert.True(publicAnnouncements.ActiveCalled);
    }

    [Theory]
    [InlineData(AdministrationApplicationErrorCodes.FeatureNotFound, StatusCodes.Status404NotFound)]
    [InlineData(AdministrationApplicationErrorCodes.SettingKeyDuplicate, StatusCodes.Status409Conflict)]
    [InlineData(AdministrationApplicationErrorCodes.SettingValueInvalid, StatusCodes.Status400BadRequest)]
    [InlineData(AdministrationApplicationErrorCodes.AnnouncementCannotDeletePublished, StatusCodes.Status409Conflict)]
    public void Exception_filter_maps_expected_status_codes(string code, int expectedStatus)
    {
        var filter = new AdministrationExceptionFilter();
        var context = CreateExceptionContext(
            new AdministrationException("failed", code));

        filter.OnException(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(expectedStatus, result.StatusCode);
        Assert.True(context.ExceptionHandled);
    }

    private static ExceptionContext CreateExceptionContext(Exception exception)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());
        return new ExceptionContext(actionContext, []) { Exception = exception };
    }

    private sealed class FakeDashboardQueries : IAdministrationDashboardQueries
    {
        public CancellationToken LastToken { get; private set; }

        public Task<AdministrationDashboardDto> GetAsync(CancellationToken cancellationToken = default)
        {
            LastToken = cancellationToken;
            return Task.FromResult(new AdministrationDashboardDto(
                new UserStatisticsDto(0, 0, 0),
                new ContentStatisticsDto(0, 0, 0, null),
                new LearningStatisticsDto(0, 0, 0, 0),
                new SearchStatisticsDto(0, 0, null),
                new OutboxStatisticsDto(0, 0, 0, 0, null, null),
                new AnalyticsDashboardStatisticsDto(0, 0, 0, 0, 0, 0, 0, 0m),
                []));
        }
    }

    private sealed class FakeFeatureFlagService : IFeatureFlagService
    {
        public Guid? LastAdministratorId { get; private set; }

        public Task<IReadOnlyList<FeatureFlagDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<FeatureFlagDto>>([]);

        public Task<FeatureFlagDto> GetByKeyAsync(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult(SampleFlag(key));

        public Task<FeatureFlagDto> CreateAsync(
            CreateFeatureFlagRequest request,
            Guid? administratorId = null,
            CancellationToken cancellationToken = default)
        {
            LastAdministratorId = administratorId;
            return Task.FromResult(SampleFlag(request.Key));
        }

        public Task<FeatureFlagDto> UpdateAsync(
            string key,
            UpdateFeatureFlagRequest request,
            Guid? administratorId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(SampleFlag(key));

        public Task<FeatureFlagDto> SetEnabledAsync(
            string key,
            bool isEnabled,
            Guid? administratorId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(SampleFlag(key) with { IsEnabled = isEnabled });

        private static FeatureFlagDto SampleFlag(string key) =>
            new(Guid.NewGuid(), key, true, null, DateTime.UtcNow, DateTime.UtcNow);
    }

    private sealed class FakeSystemSettingService : ISystemSettingService
    {
        public Guid? LastAdministratorId { get; private set; }

        public Task<IReadOnlyList<SystemSettingDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<SystemSettingDto>>([]);

        public Task<SystemSettingDto> GetByKeyAsync(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult(Sample(key));

        public Task<SystemSettingDto> CreateAsync(
            CreateSystemSettingRequest request,
            Guid? administratorId = null,
            CancellationToken cancellationToken = default)
        {
            LastAdministratorId = administratorId;
            return Task.FromResult(Sample(request.Key));
        }

        public Task<SystemSettingDto> UpdateAsync(
            string key,
            UpdateSystemSettingRequest request,
            Guid? administratorId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Sample(key));

        private static SystemSettingDto Sample(string key) =>
            new(Guid.NewGuid(), key, "v", "String", null, true, DateTime.UtcNow, DateTime.UtcNow);
    }

    private sealed class FakeAnnouncementService : IAnnouncementService
    {
        public Guid? LastAdministratorId { get; private set; }

        public bool PublishCalled { get; private set; }

        public bool ArchiveCalled { get; private set; }

        public bool DeleteCalled { get; private set; }

        public Task<AnnouncementDto> CreateAsync(
            CreateAnnouncementRequest request,
            Guid? administratorId = null,
            CancellationToken cancellationToken = default)
        {
            LastAdministratorId = administratorId;
            return Task.FromResult(Sample());
        }

        public Task<AnnouncementDto> UpdateAsync(
            Guid id,
            UpdateAnnouncementRequest request,
            Guid? administratorId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Sample(id));

        public Task<AnnouncementDto> PublishAsync(
            Guid id,
            Guid? administratorId = null,
            CancellationToken cancellationToken = default)
        {
            LastAdministratorId = administratorId;
            PublishCalled = true;
            return Task.FromResult(Sample(id));
        }

        public Task<AnnouncementDto> ArchiveAsync(
            Guid id,
            Guid? administratorId = null,
            CancellationToken cancellationToken = default)
        {
            LastAdministratorId = administratorId;
            ArchiveCalled = true;
            return Task.FromResult(Sample(id));
        }

        public Task DeleteAsync(
            Guid id,
            Guid? administratorId = null,
            CancellationToken cancellationToken = default)
        {
            LastAdministratorId = administratorId;
            DeleteCalled = true;
            return Task.CompletedTask;
        }

        public Task<AnnouncementDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Sample(id));

        public Task<AnnouncementPageDto> GetPageAsync(
            AnnouncementFilter filter,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new AnnouncementPageDto(1, 20, 0, []));

        private static AnnouncementDto Sample(Guid? id = null) =>
            new(
                id ?? Guid.NewGuid(),
                "Title",
                "Body",
                "Information",
                "Draft",
                null,
                null,
                DateTime.UtcNow,
                DateTime.UtcNow,
                null);
    }

    private sealed class FakePublicSettingsQueries : IPublicSystemSettingQueries
    {
        public bool Called { get; private set; }

        public Task<IReadOnlyList<PublicSystemSettingDto>> GetPublicAsync(
            CancellationToken cancellationToken = default)
        {
            Called = true;
            return Task.FromResult<IReadOnlyList<PublicSystemSettingDto>>([]);
        }
    }

    private sealed class FakeAnnouncementQueriesForApi : IAnnouncementQueries
    {
        public bool ActiveCalled { get; private set; }

        public Task<AnnouncementDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<AnnouncementDto?>(null);

        public Task<AnnouncementPageDto> GetPageAsync(
            AnnouncementFilter filter,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new AnnouncementPageDto(1, 20, 0, []));

        public Task<IReadOnlyList<ActiveAnnouncementDto>> GetActiveAsync(
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            ActiveCalled = true;
            return Task.FromResult<IReadOnlyList<ActiveAnnouncementDto>>([]);
        }
    }

    private sealed class FixedClock : HelpDev.SharedKernel.Time.IDateTimeProvider
    {
        public FixedClock(DateTime utcNow) => UtcNow = utcNow;

        public DateTime UtcNow { get; }
    }
}
