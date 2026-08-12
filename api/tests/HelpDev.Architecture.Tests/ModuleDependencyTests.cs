using ContentModuleMarker = HelpDev.Modules.Content.ModuleMarker;
using IdentityModuleMarker = HelpDev.Modules.Identity.ModuleMarker;
using LearningModuleMarker = HelpDev.Modules.Learning.ModuleMarker;
using NetArchTest.Rules;

namespace HelpDev.Architecture.Tests;

public sealed class ModuleDependencyTests
{
    [Fact]
    public void Content_module_does_not_depend_on_Identity_module()
    {
        AssertNoDependency(typeof(ContentModuleMarker).Assembly, "HelpDev.Modules.Identity");
    }

    [Fact]
    public void Identity_module_does_not_depend_on_Content_module()
    {
        AssertNoDependency(typeof(IdentityModuleMarker).Assembly, "HelpDev.Modules.Content");
    }

    [Fact]
    public void Learning_module_does_not_depend_on_Identity_module()
    {
        AssertNoDependency(typeof(LearningModuleMarker).Assembly, "HelpDev.Modules.Identity");
    }

    [Fact]
    public void Learning_module_does_not_depend_on_Content_module()
    {
        AssertNoDependency(typeof(LearningModuleMarker).Assembly, "HelpDev.Modules.Content");
    }

    [Fact]
    public void Identity_module_does_not_depend_on_Learning_module()
    {
        AssertNoDependency(typeof(IdentityModuleMarker).Assembly, "HelpDev.Modules.Learning");
    }

    [Fact]
    public void Content_module_does_not_depend_on_Learning_module()
    {
        AssertNoDependency(typeof(ContentModuleMarker).Assembly, "HelpDev.Modules.Learning");
    }

    [Fact]
    public void Content_module_does_not_depend_on_legacy_projects()
    {
        AssertNoLegacyDependencies(typeof(ContentModuleMarker).Assembly);
    }

    [Fact]
    public void Identity_module_does_not_depend_on_legacy_projects()
    {
        AssertNoLegacyDependencies(typeof(IdentityModuleMarker).Assembly);
    }

    [Fact]
    public void Learning_module_does_not_depend_on_legacy_projects()
    {
        AssertNoLegacyDependencies(typeof(LearningModuleMarker).Assembly);
    }

    [Fact]
    public void Learning_Application_does_not_depend_on_host_Infrastructure()
    {
        var result = Types.InAssembly(typeof(LearningModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application")
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    private static void AssertNoDependency(System.Reflection.Assembly assembly, string dependency)
    {
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(dependency)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    private static void AssertNoLegacyDependencies(System.Reflection.Assembly assembly)
    {
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "HelpDev.Domain",
                "HelpDev.Application",
                "HelpDev.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}
