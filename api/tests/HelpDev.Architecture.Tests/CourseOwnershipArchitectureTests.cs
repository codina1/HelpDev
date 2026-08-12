using System.Security.Claims;
using HelpDev.API.Controllers;
using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Application.Courses.Dtos;
using NetArchTest.Rules;

namespace HelpDev.Architecture.Tests;

public sealed class CourseOwnershipArchitectureTests
{
    [Fact]
    public void CourseManagementActor_does_not_depend_on_ASPNET_Core()
    {
        var result = Types.InAssembly(typeof(CourseManagementActor).Assembly)
            .That()
            .HaveName(nameof(CourseManagementActor))
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
        Assert.DoesNotContain(
            typeof(CourseManagementActor).Assembly.GetReferencedAssemblies(),
            assembly => assembly.Name?.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal) == true);
    }

    [Fact]
    public void Learning_Application_does_not_depend_on_ClaimsPrincipal()
    {
        var result = Types.InAssembly(typeof(ICourseService).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application")
            .ShouldNot()
            .HaveDependencyOn(typeof(ClaimsPrincipal).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Learning_Application_does_not_depend_on_Identity_roles()
    {
        var result = Types.InAssembly(typeof(ICourseService).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application")
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Modules.Identity")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Management_controller_does_not_access_repositories_or_DbContext()
    {
        var ctor = typeof(LearningCourseManagementController).GetConstructors().Single();
        Assert.Single(ctor.GetParameters());
        Assert.Equal(typeof(ICourseService), ctor.GetParameters()[0].ParameterType);
    }

    [Fact]
    public void Course_ownership_flags_are_not_exposed_in_request_contracts()
    {
        Assert.Null(typeof(CreateCourseRequest).GetProperty("CanManageAllCourses"));
        Assert.Null(typeof(UpdateCourseRequest).GetProperty("CanManageAllCourses"));
        Assert.Null(typeof(CreateCourseRequest).GetProperty("InstructorId"));
    }

    [Fact]
    public void Management_controller_remains_protected_by_WriterOrAdmin()
    {
        var attribute = Assert.Single(
            typeof(LearningCourseManagementController)
                .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), inherit: true)
                .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>());

        Assert.Equal(
            HelpDev.Modules.Identity.Application.Auth.AuthorizationPolicies.WriterOrAdmin,
            attribute.Policy);
    }

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}
