using HelpDev.API.Controllers;
using HelpDev.Modules.Learning.Application.Courses.Dtos;
using NetArchTest.Rules;

namespace HelpDev.Architecture.Tests;

public sealed class LearningCourseApiArchitectureTests
{
    [Fact]
    public void Course_api_controllers_do_not_depend_on_learning_infrastructure()
    {
        var result = Types.InAssembly(typeof(LearningCoursesController).Assembly)
            .That()
            .HaveNameStartingWith("LearningCourse")
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Modules.Learning.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Course_api_controllers_do_not_depend_on_ApplicationDbContext()
    {
        var result = Types.InAssembly(typeof(LearningCoursesController).Assembly)
            .That()
            .HaveNameStartingWith("LearningCourse")
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Infrastructure.Persistence")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Public_course_dtos_do_not_expose_domain_types()
    {
        Assert.DoesNotContain(
            typeof(CourseDetailDto).GetProperties(),
            property => property.PropertyType.Namespace?.Contains(".Domain.") == true);
        Assert.DoesNotContain(
            typeof(CourseListItemDto).GetProperties(),
            property => property.PropertyType.Namespace?.Contains(".Domain.") == true);
        Assert.Null(typeof(CreateCourseRequest).GetProperty("InstructorId"));
    }

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}
