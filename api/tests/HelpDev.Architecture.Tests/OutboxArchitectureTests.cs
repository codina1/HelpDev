using HelpDev.API.Controllers;
using HelpDev.Infrastructure.Outbox;
using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.SharedApplication.Abstractions.Events;
using HelpDev.SharedInfrastructure.Outbox;
using HelpDev.SharedKernel.Results;
using NetArchTest.Rules;

namespace HelpDev.Architecture.Tests;

public sealed class OutboxArchitectureTests
{
    [Fact]
    public void SharedKernel_does_not_depend_on_Outbox_Infrastructure()
    {
        var result = Types.InAssembly(typeof(Error).Assembly)
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Infrastructure.Outbox")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void SharedApplication_does_not_depend_on_Outbox_Infrastructure()
    {
        var result = Types.InAssembly(typeof(IDomainEventDispatcher).Assembly)
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Infrastructure.Outbox")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Learning_Application_does_not_depend_on_OutboxMessage()
    {
        var result = Types.InAssembly(typeof(ICourseService).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application")
            .ShouldNot()
            .HaveDependencyOn(typeof(OutboxMessage).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Non_outbox_api_controllers_do_not_depend_on_Outbox_processor_store_or_entity()
    {
        var result = Types.InAssembly(typeof(LearningCoursesController).Assembly)
            .That()
            .ResideInNamespace("HelpDev.API.Controllers")
            .And()
            .DoNotHaveName("OutboxManagementController")
            .ShouldNot()
            .HaveDependencyOnAny(
                typeof(IOutboxMessageStore).FullName!,
                typeof(OutboxProcessor).FullName!,
                typeof(OutboxMessage).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Outbox_SharedInfrastructure_does_not_depend_on_Learning_or_Content_modules()
    {
        var result = Types.InAssembly(typeof(IOutboxEventSerializer).Assembly)
            .That()
            .ResideInNamespaceContaining(".Outbox")
            .ShouldNot()
            .HaveDependencyOnAny(
                "HelpDev.Modules.Learning",
                "HelpDev.Modules.Content")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void OutboxProcessor_does_not_take_ApplicationDbContext_in_constructor()
    {
        var ctor = typeof(OutboxProcessor).GetConstructors().Single();
        Assert.DoesNotContain(
            ctor.GetParameters(),
            parameter => parameter.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal));
    }

    [Fact]
    public void Application_services_do_not_depend_on_IDomainEventDispatcher()
    {
        var result = Types.InAssembly(typeof(ICourseService).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application")
            .ShouldNot()
            .HaveDependencyOn(typeof(IDomainEventDispatcher).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}
