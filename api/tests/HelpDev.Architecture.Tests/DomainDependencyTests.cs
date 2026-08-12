using ContentModuleMarker = HelpDev.Modules.Content.ModuleMarker;
using IdentityModuleMarker = HelpDev.Modules.Identity.ModuleMarker;
using LearningModuleMarker = HelpDev.Modules.Learning.ModuleMarker;
using NetArchTest.Rules;

namespace HelpDev.Architecture.Tests;

public sealed class DomainDependencyTests
{
    [Fact]
    public void Content_domain_does_not_depend_on_EntityFrameworkCore()
    {
        AssertDomainDoesNotDependOn(typeof(ContentModuleMarker).Assembly, "Microsoft.EntityFrameworkCore");
    }

    [Fact]
    public void Content_domain_does_not_depend_on_AspNetCore()
    {
        AssertDomainDoesNotDependOn(typeof(ContentModuleMarker).Assembly, "Microsoft.AspNetCore");
    }

    [Fact]
    public void Identity_domain_does_not_depend_on_EntityFrameworkCore()
    {
        AssertDomainDoesNotDependOn(typeof(IdentityModuleMarker).Assembly, "Microsoft.EntityFrameworkCore");
    }

    [Fact]
    public void Identity_domain_does_not_depend_on_AspNetCore()
    {
        AssertDomainDoesNotDependOn(typeof(IdentityModuleMarker).Assembly, "Microsoft.AspNetCore");
    }

    [Fact]
    public void Learning_domain_does_not_depend_on_EntityFrameworkCore()
    {
        AssertDomainDoesNotDependOn(typeof(LearningModuleMarker).Assembly, "Microsoft.EntityFrameworkCore");
    }

    [Fact]
    public void Learning_domain_does_not_depend_on_AspNetCore()
    {
        AssertDomainDoesNotDependOn(typeof(LearningModuleMarker).Assembly, "Microsoft.AspNetCore");
    }

    private static void AssertDomainDoesNotDependOn(System.Reflection.Assembly assembly, string dependency)
    {
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceContaining(".Domain")
            .ShouldNot()
            .HaveDependencyOn(dependency)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}
