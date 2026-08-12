using System.Reflection;
using HelpDev.API.Controllers;
using HelpDev.API.Filters;
using HelpDev.API.Tests.Fakes;
using HelpDev.Infrastructure.Outbox.Operations;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.SharedApplication.Abstractions.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace HelpDev.API.Tests;

public sealed class OutboxManagementControllerTests
{
    [Fact]
    public void Controller_requires_AdminOnly_not_WriterOrAdmin()
    {
        var attribute = Assert.Single(
            typeof(OutboxManagementController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>());

        Assert.Equal(AuthorizationPolicies.AdminOnly, attribute.Policy);
        Assert.NotEqual(AuthorizationPolicies.WriterOrAdmin, attribute.Policy);
        Assert.NotNull(attribute); // anonymous blocked by Authorize
    }

    [Fact]
    public void Controller_depends_only_on_operations_abstractions()
    {
        var parameters = typeof(OutboxManagementController).GetConstructors().Single().GetParameters();
        Assert.Equal(2, parameters.Length);
        Assert.Contains(parameters, p => p.ParameterType == typeof(IOutboxOperationsQueries));
        Assert.Contains(parameters, p => p.ParameterType == typeof(IOutboxOperationsService));
        Assert.DoesNotContain(
            parameters,
            p => p.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal)
                || p.ParameterType.Name.Contains("Dispatcher", StringComparison.Ordinal)
                || p.ParameterType.Name.Contains("Serializer", StringComparison.Ordinal)
                || p.ParameterType.Name.Contains("Processor", StringComparison.Ordinal)
                || p.ParameterType == typeof(IOutboxRetryStore));
    }

    [Fact]
    public async Task Admin_can_get_status_and_list_forwards_filters()
    {
        var queries = new FakeOutboxOperationsQueries();
        var service = new FakeOutboxOperationsService();
        var controller = new OutboxManagementController(queries, service);
        ControllerTestHelper.SetUser(controller, Guid.NewGuid(), AppRoles.Admin);

        var statusResult = await controller.GetStatus(CancellationToken.None);
        Assert.IsType<OkObjectResult>(statusResult.Result);

        await controller.ListMessages("failed", "content.published.v1", 2, 15, CancellationToken.None);
        Assert.Equal("failed", queries.LastFilter!.Status);
        Assert.Equal("content.published.v1", queries.LastFilter.Type);
        Assert.Equal(2, queries.LastFilter.Page);
        Assert.Equal(15, queries.LastFilter.PageSize);
    }

    [Fact]
    public async Task Detail_missing_maps_to_not_found_exception()
    {
        var queries = new FakeOutboxOperationsQueries { Detail = null };
        var controller = new OutboxManagementController(queries, new FakeOutboxOperationsService());
        ControllerTestHelper.SetUser(controller, Guid.NewGuid(), AppRoles.Admin);

        var ex = await Assert.ThrowsAsync<OutboxOperationsException>(() =>
            controller.GetMessage(Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(OutboxOperationsErrorCodes.MessageNotFound, ex.Code);
    }

    [Fact]
    public async Task Retry_success_returns_200_and_forwards_admin_id()
    {
        var service = new FakeOutboxOperationsService();
        var controller = new OutboxManagementController(new FakeOutboxOperationsQueries(), service);
        var adminId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, adminId, AppRoles.Admin);
        var messageId = Guid.NewGuid();

        var result = await controller.RetryMessage(messageId, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(messageId, service.LastRetryId);
        Assert.Equal(adminId, service.LastAdministratorId);
    }

    [Fact]
    public async Task Batch_retry_defaults_limit_and_rejects_via_service()
    {
        var service = new FakeOutboxOperationsService
        {
            ExceptionToThrow = new OutboxOperationsException(
                "bad",
                OutboxOperationsErrorCodes.RetryLimitInvalid),
        };
        var controller = new OutboxManagementController(new FakeOutboxOperationsQueries(), service);
        ControllerTestHelper.SetUser(controller, Guid.NewGuid(), AppRoles.Admin);

        await Assert.ThrowsAsync<OutboxOperationsException>(() =>
            controller.RetryFailed(new RetryFailedOutboxHttpRequest { Limit = 999 }, CancellationToken.None));

        service.ExceptionToThrow = null;
        await controller.RetryFailed(null, CancellationToken.None);
        Assert.Equal(OutboxOperationsService.DefaultRetryFailedLimit, service.LastBatchRequest!.Limit);
    }

    [Fact]
    public void Request_and_dto_contracts_do_not_expose_payload_or_lock_id()
    {
        Assert.Null(typeof(RetryFailedOutboxHttpRequest).GetProperty("Payload"));
        Assert.Null(typeof(RetryFailedOutboxHttpRequest).GetProperty("LockId"));
        Assert.Null(typeof(RetryFailedOutboxHttpRequest).GetProperty("TypeReplacement"));
        Assert.Null(typeof(OutboxMessageDetailDto).GetProperty("Payload"));
        Assert.Null(typeof(OutboxMessageDetailDto).GetProperty("LockId"));
        Assert.Null(typeof(OutboxMessageListItemDto).GetProperty("Payload"));
        Assert.Null(typeof(OutboxMessageListItemDto).GetProperty("LockId"));

        var requestNames = typeof(RetryFailedOutboxHttpRequest)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(p => p.Name)
            .OrderBy(n => n)
            .ToArray();
        Assert.Equal(["Limit", "Type"], requestNames);
    }

    [Fact]
    public void Controller_does_not_reference_IDomainEventDispatcher()
    {
        var result = typeof(OutboxManagementController).Assembly
            .GetType(typeof(OutboxManagementController).FullName!)!
            .GetConstructors()
            .SelectMany(ctor => ctor.GetParameters())
            .Any(parameter => parameter.ParameterType == typeof(IDomainEventDispatcher));

        Assert.False(result);
    }
}

public sealed class OutboxOperationsExceptionFilterTests
{
    [Theory]
    [InlineData(OutboxOperationsErrorCodes.MessageNotFound, StatusCodes.Status404NotFound)]
    [InlineData(OutboxOperationsErrorCodes.MessageAlreadyProcessed, StatusCodes.Status409Conflict)]
    [InlineData(OutboxOperationsErrorCodes.MessageCurrentlyProcessing, StatusCodes.Status409Conflict)]
    [InlineData(OutboxOperationsErrorCodes.PageInvalid, StatusCodes.Status400BadRequest)]
    [InlineData(OutboxOperationsErrorCodes.RetryLimitInvalid, StatusCodes.Status400BadRequest)]
    public void Filter_maps_codes(string code, int expectedStatus)
    {
        var filter = new OutboxOperationsExceptionFilter();
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = new OutboxOperationsException("boom", code),
        };

        filter.OnException(context);

        Assert.True(context.ExceptionHandled);
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(expectedStatus, result.StatusCode);
    }
}

internal sealed class FakeOutboxOperationsQueries : IOutboxOperationsQueries
{
    public OutboxMessageFilter? LastFilter { get; private set; }

    public OutboxMessageDetailDto? Detail { get; set; } =
        new(
            Guid.NewGuid(),
            "content.published.v1",
            DateTime.UtcNow,
            null,
            0,
            null,
            null,
            null,
            OutboxMessageStatuses.Pending);

    public Task<OutboxStatusDto> GetStatusAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(new OutboxStatusDto(1, 0, 0, 2, DateTime.UtcNow, DateTime.UtcNow));

    public Task<OutboxMessagePageDto> ListAsync(
        OutboxMessageFilter filter,
        CancellationToken cancellationToken = default)
    {
        LastFilter = filter;
        return Task.FromResult(new OutboxMessagePageDto(filter.Page, filter.PageSize, 0, []));
    }

    public Task<OutboxMessageDetailDto?> GetByIdAsync(
        Guid messageId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Detail);
}

internal sealed class FakeOutboxOperationsService : IOutboxOperationsService
{
    public Guid? LastRetryId { get; private set; }

    public Guid? LastAdministratorId { get; private set; }

    public RetryFailedOutboxRequest? LastBatchRequest { get; private set; }

    public Exception? ExceptionToThrow { get; set; }

    public Task<OutboxMessageDetailDto> RetryAsync(
        Guid messageId,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default)
    {
        LastRetryId = messageId;
        LastAdministratorId = administratorId;
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        return Task.FromResult(new OutboxMessageDetailDto(
            messageId,
            "content.published.v1",
            DateTime.UtcNow,
            null,
            0,
            null,
            null,
            null,
            OutboxMessageStatuses.Pending));
    }

    public Task<RetryFailedOutboxResultDto> RetryFailedAsync(
        RetryFailedOutboxRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default)
    {
        LastBatchRequest = request;
        LastAdministratorId = administratorId;
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        return Task.FromResult(new RetryFailedOutboxResultDto(
            request.Limit,
            0,
            request.Type,
            DateTime.UtcNow));
    }
}
