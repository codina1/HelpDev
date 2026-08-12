using System.Reflection;
using HelpDev.API.Controllers;
using HelpDev.API.Filters;
using HelpDev.API.Security;
using HelpDev.API.Tests.Fakes;
using HelpDev.Modules.Auditing.Application.Queries;
using HelpDev.Modules.Auditing.Domain;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.SharedContracts.Auditing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace HelpDev.API.Tests;

public sealed class AuditApiTests
{
    [Fact]
    public void Audit_admin_controller_requires_AdminOnly_policy()
    {
        var attribute = Assert.Single(
            typeof(AuditAdminController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>());

        Assert.Equal(AuthorizationPolicies.AdminOnly, attribute.Policy);
    }

    [Fact]
    public void Audit_admin_controller_depends_on_query_abstractions_not_repositories()
    {
        var parameters = typeof(AuditAdminController).GetConstructors().Single().GetParameters();
        Assert.Single(parameters);
        Assert.Equal(typeof(IAuditQueries), parameters[0].ParameterType);
    }

    [Fact]
    public async Task GetPage_forwards_filter_to_queries()
    {
        var queries = new FakeAuditQueries();
        var controller = new AuditAdminController(queries);
        ControllerTestHelper.SetUser(controller, Guid.NewGuid(), AppRoles.Admin);

        var result = await controller.GetPage(
            from: null,
            to: null,
            category: AuditCategories.Administration,
            action: AuditActions.AdministrationFeatureFlagCreated,
            outcome: AuditOutcomes.Success,
            actorUserId: null,
            subjectId: null,
            subjectType: null,
            page: 1,
            pageSize: 20,
            cancellationToken: CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(1, queries.PageCallCount);
        Assert.Equal(AuditCategories.Administration, queries.LastFilter!.Category);
    }

    [Fact]
    public void Audit_exception_filter_maps_not_found_to_404()
    {
        var filter = new AuditExceptionFilter();
        var context = CreateExceptionContext(new AuditException("missing", AuditErrorCodes.RecordNotFound));

        filter.OnException(context);

        Assert.True(context.ExceptionHandled);
        var objectResult = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
    }

    private static ExceptionContext CreateExceptionContext(Exception exception)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());

        return new ExceptionContext(actionContext, [])
        {
            Exception = exception,
        };
    }

    private sealed class FakeAuditQueries : IAuditQueries
    {
        public int PageCallCount { get; private set; }

        public AuditQueryFilter? LastFilter { get; private set; }

        public Task<AuditPageResult> GetPageAsync(AuditQueryFilter filter, CancellationToken cancellationToken = default)
        {
            PageCallCount++;
            LastFilter = filter;
            return Task.FromResult(new AuditPageResult([], filter.Page, filter.PageSize, 0));
        }

        public Task<AuditRecordDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
