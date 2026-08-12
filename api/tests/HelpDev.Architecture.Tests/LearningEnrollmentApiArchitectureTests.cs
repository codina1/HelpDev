using HelpDev.API.Controllers;
using HelpDev.Modules.Learning.Application.Enrollments.Dtos;
using HelpDev.SharedApplication.Abstractions.Events;
using NetArchTest.Rules;

namespace HelpDev.Architecture.Tests;

public sealed class LearningEnrollmentApiArchitectureTests
{
    [Fact]
    public void Enrollment_api_controller_does_not_depend_on_learning_infrastructure()
    {
        var result = Types.InAssembly(typeof(LearningEnrollmentsController).Assembly)
            .That()
            .HaveName(nameof(LearningEnrollmentsController))
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Modules.Learning.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Enrollment_api_controller_does_not_depend_on_ApplicationDbContext()
    {
        var result = Types.InAssembly(typeof(LearningEnrollmentsController).Assembly)
            .That()
            .HaveName(nameof(LearningEnrollmentsController))
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Infrastructure.Persistence")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Enrollment_api_controller_does_not_depend_on_repositories()
    {
        var ctor = typeof(LearningEnrollmentsController).GetConstructors().Single();
        Assert.DoesNotContain(
            ctor.GetParameters(),
            parameter => parameter.ParameterType.Name.Contains("Repository", StringComparison.Ordinal));
    }

    [Fact]
    public void Enrollment_api_controller_does_not_depend_on_IDomainEventDispatcher()
    {
        var result = Types.InAssembly(typeof(LearningEnrollmentsController).Assembly)
            .That()
            .HaveName(nameof(LearningEnrollmentsController))
            .ShouldNot()
            .HaveDependencyOn(typeof(IDomainEventDispatcher).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Enrollment_http_actions_do_not_accept_UserId_from_body_or_route_parameters_named_userId()
    {
        var methods = typeof(LearningEnrollmentsController)
            .GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly);

        foreach (var method in methods)
        {
            Assert.DoesNotContain(
                method.GetParameters(),
                parameter =>
                    parameter.Name is not null
                    && parameter.Name.Equals("userId", StringComparison.OrdinalIgnoreCase));

            Assert.DoesNotContain(
                method.GetParameters(),
                parameter => parameter.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.FromBodyAttribute), inherit: true).Any());
        }
    }

    [Fact]
    public void Enrollment_response_dtos_do_not_expose_domain_types()
    {
        Assert.DoesNotContain(
            typeof(EnrollmentDto).GetProperties(),
            property => property.PropertyType.Namespace?.Contains(".Domain.") == true);
        Assert.DoesNotContain(
            typeof(EnrollmentListItemDto).GetProperties(),
            property => property.PropertyType.Namespace?.Contains(".Domain.") == true);
        Assert.DoesNotContain(
            typeof(LessonProgressDto).GetProperties(),
            property => property.PropertyType.Namespace?.Contains(".Domain.") == true);
    }

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}
