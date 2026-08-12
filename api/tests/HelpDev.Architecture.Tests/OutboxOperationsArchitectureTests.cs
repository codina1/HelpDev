using System.Reflection;
using HelpDev.API.Controllers;
using HelpDev.Infrastructure.Outbox;
using HelpDev.Infrastructure.Outbox.Operations;
using HelpDev.Infrastructure.Persistence;
using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Search.Application.Search;
using HelpDev.SharedApplication.Abstractions.Events;
using HelpDev.SharedInfrastructure.Outbox;
using NetArchTest.Rules;

namespace HelpDev.Architecture.Tests;

public sealed class OutboxOperationsArchitectureTests
{
    [Fact]
    public void Outbox_management_controller_does_not_depend_on_Infrastructure_concrete_types()
    {
        var ctor = typeof(OutboxManagementController).GetConstructors().Single();
        Assert.DoesNotContain(
            ctor.GetParameters(),
            parameter =>
                parameter.ParameterType == typeof(ApplicationDbContext)
                || parameter.ParameterType == typeof(OutboxMessageStore)
                || parameter.ParameterType == typeof(OutboxProcessor)
                || parameter.ParameterType == typeof(OutboxMessage)
                || parameter.ParameterType == typeof(EfOutboxRetryStore)
                || parameter.ParameterType == typeof(IOutboxEventSerializer));
    }

    [Fact]
    public void Controllers_do_not_depend_on_IDomainEventDispatcher()
    {
        var result = Types.InAssembly(typeof(OutboxManagementController).Assembly)
            .That()
            .ResideInNamespace("HelpDev.API.Controllers")
            .ShouldNot()
            .HaveDependencyOn(typeof(IDomainEventDispatcher).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Outbox_operations_DTOs_do_not_expose_OutboxMessage_or_Payload()
    {
        foreach (var dtoType in new[]
                 {
                     typeof(OutboxStatusDto),
                     typeof(OutboxMessageListItemDto),
                     typeof(OutboxMessageDetailDto),
                     typeof(OutboxMessagePageDto),
                     typeof(RetryFailedOutboxResultDto),
                 })
        {
            Assert.Null(dtoType.GetProperty("Payload"));
            Assert.Null(dtoType.GetProperty("LockId"));
            Assert.All(
                dtoType.GetProperties(BindingFlags.Instance | BindingFlags.Public),
                property => Assert.NotEqual(typeof(OutboxMessage), property.PropertyType));
        }
    }

    [Fact]
    public void Recovery_service_does_not_depend_on_serializer_or_handler_types()
    {
        var ctor = typeof(OutboxOperationsService).GetConstructors().Single();
        Assert.DoesNotContain(
            ctor.GetParameters(),
            parameter =>
                parameter.ParameterType == typeof(IOutboxEventSerializer)
                || parameter.ParameterType == typeof(IDomainEventDispatcher)
                || parameter.ParameterType.Name.Contains("Handler", StringComparison.Ordinal));
    }

    [Fact]
    public void Module_Application_services_do_not_depend_on_Outbox_operations()
    {
        var result = Types.InAssembly(typeof(ICourseService).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application")
            .ShouldNot()
            .HaveDependencyOnAny(
                typeof(IOutboxOperationsService).FullName!,
                typeof(IOutboxOperationsQueries).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Public_module_APIs_do_not_expose_Outbox_operations()
    {
        var result = Types.InAssembly(typeof(SearchController).Assembly)
            .That()
            .HaveName(nameof(SearchController))
            .Or()
            .HaveName(nameof(LearningCoursesController))
            .ShouldNot()
            .HaveDependencyOnAny(
                typeof(IOutboxOperationsService).FullName!,
                typeof(IOutboxOperationsQueries).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Retry_code_does_not_create_OutboxMessage_via_public_API_requests()
    {
        Assert.Null(typeof(RetryFailedOutboxHttpRequest).GetProperty("Payload"));
        Assert.Null(typeof(RetryFailedOutboxHttpRequest).GetProperty("Id"));
        Assert.Null(typeof(RetryFailedOutboxHttpRequest).GetMethod("Create"));
    }

    [Fact]
    public void Non_outbox_controllers_do_not_depend_on_OutboxMessage_store_or_processor()
    {
        var result = Types.InAssembly(typeof(LearningCoursesController).Assembly)
            .That()
            .ResideInNamespace("HelpDev.API.Controllers")
            .And()
            .DoNotHaveName(nameof(OutboxManagementController))
            .ShouldNot()
            .HaveDependencyOnAny(
                typeof(IOutboxMessageStore).FullName!,
                typeof(OutboxProcessor).FullName!,
                typeof(OutboxMessage).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}
