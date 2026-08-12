using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Application.Enrollments.Dtos;
using LearningModuleMarker = HelpDev.Modules.Learning.ModuleMarker;
using NetArchTest.Rules;

namespace HelpDev.Architecture.Tests;

public sealed class LearningApplicationArchitectureTests
{
    [Fact]
    public void Learning_Application_Courses_does_not_depend_on_EntityFrameworkCore()
    {
        AssertNamespaceDoesNotDependOn(".Application.Courses", "Microsoft.EntityFrameworkCore");
    }

    [Fact]
    public void Learning_Application_Courses_does_not_depend_on_AspNetCore()
    {
        AssertNamespaceDoesNotDependOn(".Application.Courses", "Microsoft.AspNetCore");
    }

    [Fact]
    public void Learning_Application_Enrollments_does_not_depend_on_EntityFrameworkCore()
    {
        AssertNamespaceDoesNotDependOn(".Application.Enrollments", "Microsoft.EntityFrameworkCore");
    }

    [Fact]
    public void Learning_Application_Enrollments_does_not_depend_on_AspNetCore()
    {
        AssertNamespaceDoesNotDependOn(".Application.Enrollments", "Microsoft.AspNetCore");
    }

    [Fact]
    public void Learning_Application_Enrollments_does_not_depend_on_Identity_or_Content()
    {
        var result = Types.InAssembly(typeof(LearningModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application.Enrollments")
            .ShouldNot()
            .HaveDependencyOnAny("HelpDev.Modules.Identity", "HelpDev.Modules.Content")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Learning_Application_does_not_depend_on_Identity_or_Content()
    {
        var result = Types.InAssembly(typeof(LearningModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application")
            .ShouldNot()
            .HaveDependencyOnAny("HelpDev.Modules.Identity", "HelpDev.Modules.Content")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Learning_Infrastructure_does_not_depend_on_Identity_or_Content()
    {
        var result = Types.InAssembly(typeof(LearningModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Infrastructure")
            .ShouldNot()
            .HaveDependencyOnAny("HelpDev.Modules.Identity", "HelpDev.Modules.Content")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Learning_course_dtos_do_not_expose_domain_entity_types()
    {
        AssertDtosDoNotDependOnDomain(
            ".Application.Courses.Dtos",
            typeof(HelpDev.Modules.Learning.Domain.Courses.Course).FullName!,
            typeof(HelpDev.Modules.Learning.Domain.Courses.Section).FullName!,
            typeof(HelpDev.Modules.Learning.Domain.Courses.Lesson).FullName!);

        Assert.DoesNotContain(
            typeof(CourseDetailDto).GetProperties(),
            property => property.PropertyType.Namespace?.Contains(".Domain.") == true);
        Assert.DoesNotContain(
            typeof(CourseListItemDto).GetProperties(),
            property => property.PropertyType.Namespace?.Contains(".Domain.") == true);
    }

    [Fact]
    public void Learning_enrollment_dtos_do_not_expose_domain_entity_types()
    {
        AssertDtosDoNotDependOnDomain(
            ".Application.Enrollments.Dtos",
            typeof(HelpDev.Modules.Learning.Domain.Enrollments.Enrollment).FullName!,
            typeof(HelpDev.Modules.Learning.Domain.Enrollments.LessonProgress).FullName!,
            typeof(HelpDev.Modules.Learning.Domain.Enrollments.ProgressPercentage).FullName!);

        Assert.DoesNotContain(
            typeof(EnrollmentDto).GetProperties(),
            property => property.PropertyType.Namespace?.Contains(".Domain.") == true);
        Assert.DoesNotContain(
            typeof(LessonProgressDto).GetProperties(),
            property => property.PropertyType.Namespace?.Contains(".Domain.") == true);
        Assert.DoesNotContain(
            typeof(EnrollmentListItemDto).GetProperties(),
            property => property.PropertyType.Namespace?.Contains(".Domain.") == true);
    }

    private static void AssertNamespaceDoesNotDependOn(string namespaceFragment, string dependency)
    {
        var result = Types.InAssembly(typeof(LearningModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(namespaceFragment)
            .ShouldNot()
            .HaveDependencyOn(dependency)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    private static void AssertDtosDoNotDependOnDomain(string dtoNamespace, params string[] domainTypes)
    {
        var result = Types.InAssembly(typeof(LearningModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(dtoNamespace)
            .ShouldNot()
            .HaveDependencyOnAny(domainTypes)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}
