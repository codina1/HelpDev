using HelpDev.SharedKernel.Results;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Modules;
using HelpDev.SharedInfrastructure;
using NetArchTest.Rules;

namespace HelpDev.Architecture.Tests;

public sealed class BuildingBlockDependencyTests
{
    private static readonly string[] ModuleAssemblies =
    [
        "HelpDev.Modules.Identity",
        "HelpDev.Modules.Content",
        "HelpDev.Modules.Learning",
        "HelpDev.Modules.Toolbox",
        "HelpDev.Modules.PromptLab",
        "HelpDev.Modules.Search",
        "HelpDev.Modules.Analytics",
        "HelpDev.Modules.Administration",
    ];

    [Fact]
    public void SharedKernel_does_not_depend_on_any_module()
    {
        var result = Types.InAssembly(typeof(Error).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(ModuleAssemblies)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void SharedApplication_does_not_depend_on_any_module()
    {
        var result = Types.InAssembly(typeof(IUnitOfWork).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(ModuleAssemblies)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void SharedContracts_does_not_depend_on_any_module()
    {
        var result = Types.InAssembly(typeof(IModule).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(ModuleAssemblies)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void SharedInfrastructure_does_not_depend_on_any_module()
    {
        var result = Types.InAssembly(typeof(DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(ModuleAssemblies)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || result.FailingTypeNames.Count() == 0
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}
