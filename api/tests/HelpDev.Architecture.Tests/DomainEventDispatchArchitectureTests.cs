using HelpDev.API.Controllers;
using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.SharedApplication.Abstractions.Events;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedInfrastructure.Events;
using HelpDev.SharedKernel.Results;
using NetArchTest.Rules;

namespace HelpDev.Architecture.Tests;

public sealed class DomainEventDispatchArchitectureTests
{
    [Fact]
    public void SharedKernel_does_not_depend_on_SharedInfrastructure()
    {
        var result = Types.InAssembly(typeof(Error).Assembly)
            .ShouldNot()
            .HaveDependencyOn("HelpDev.SharedInfrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void SharedApplication_does_not_depend_on_SharedInfrastructure()
    {
        var result = Types.InAssembly(typeof(IUnitOfWork).Assembly)
            .ShouldNot()
            .HaveDependencyOn("HelpDev.SharedInfrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Module_domain_types_do_not_depend_on_DomainEventDispatcher()
    {
        var assemblies = new[]
        {
            typeof(HelpDev.Modules.Content.ModuleMarker).Assembly,
            typeof(HelpDev.Modules.Identity.ModuleMarker).Assembly,
            typeof(HelpDev.Modules.Learning.ModuleMarker).Assembly,
        };

        foreach (var assembly in assemblies)
        {
            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespaceContaining(".Domain")
                .ShouldNot()
                .HaveDependencyOn(typeof(DomainEventDispatcher).FullName!)
                .GetResult();

            Assert.True(result.IsSuccessful, FormatFailures(result));
        }
    }

    [Fact]
    public void Learning_application_services_do_not_depend_on_DomainEventDispatcher_concrete_type()
    {
        var result = Types.InAssembly(typeof(ICourseService).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application")
            .ShouldNot()
            .HaveDependencyOn(typeof(DomainEventDispatcher).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Api_controllers_do_not_depend_on_IDomainEventDispatcher()
    {
        var result = Types.InAssembly(typeof(LearningCoursesController).Assembly)
            .That()
            .ResideInNamespace("HelpDev.API.Controllers")
            .ShouldNot()
            .HaveDependencyOn(typeof(IDomainEventDispatcher).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void DomainEventDispatcher_does_not_depend_on_Identity_Content_or_Learning()
    {
        var result = Types.InAssembly(typeof(DomainEventDispatcher).Assembly)
            .That()
            .HaveName(nameof(DomainEventDispatcher))
            .ShouldNot()
            .HaveDependencyOnAny(
                "HelpDev.Modules.Identity",
                "HelpDev.Modules.Content",
                "HelpDev.Modules.Learning")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}
