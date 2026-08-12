using System.Reflection;
using HelpDev.API.Controllers;
using HelpDev.API.Filters;
using HelpDev.API.Tests.Fakes;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Learning.Application.Enrollments;
using HelpDev.Modules.Learning.Application.Enrollments.Dtos;
using HelpDev.SharedApplication.Abstractions.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace HelpDev.API.Tests;

public sealed class LearningEnrollmentsControllerTests
{
    [Fact]
    public void Controller_requires_authenticated_policy()
    {
        var attribute = Assert.Single(
            typeof(LearningEnrollmentsController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>());

        Assert.Equal(AuthorizationPolicies.Authenticated, attribute.Policy);
    }

    [Theory]
    [InlineData(nameof(LearningEnrollmentsController.Enroll))]
    [InlineData(nameof(LearningEnrollmentsController.ListMine))]
    [InlineData(nameof(LearningEnrollmentsController.GetById))]
    [InlineData(nameof(LearningEnrollmentsController.GetByCourse))]
    [InlineData(nameof(LearningEnrollmentsController.StartLesson))]
    [InlineData(nameof(LearningEnrollmentsController.CompleteLesson))]
    public void Every_endpoint_inherits_authorize_metadata(string methodName)
    {
        var method = typeof(LearningEnrollmentsController).GetMethod(methodName);
        Assert.NotNull(method);

        var classAuthorize = typeof(LearningEnrollmentsController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true);
        Assert.NotEmpty(classAuthorize);
    }

    [Fact]
    public async Task Missing_user_id_returns_unauthorized_for_all_actions()
    {
        var service = new FakeEnrollmentService();
        var controller = new LearningEnrollmentsController(service);
        ControllerTestHelper.SetUser(controller, userId: null);
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();

        Assert.IsType<UnauthorizedResult>((await controller.Enroll(courseId, CancellationToken.None)).Result);
        Assert.IsType<UnauthorizedResult>((await controller.ListMine(CancellationToken.None)).Result);
        Assert.IsType<UnauthorizedResult>((await controller.GetById(enrollmentId, CancellationToken.None)).Result);
        Assert.IsType<UnauthorizedResult>((await controller.GetByCourse(courseId, CancellationToken.None)).Result);
        Assert.IsType<UnauthorizedResult>((await controller.StartLesson(courseId, lessonId, CancellationToken.None)).Result);
        Assert.IsType<UnauthorizedResult>((await controller.CompleteLesson(courseId, lessonId, CancellationToken.None)).Result);
        Assert.Null(service.LastOperation);
    }

    [Fact]
    public async Task Malformed_user_id_returns_unauthorized()
    {
        var service = new FakeEnrollmentService();
        var controller = new LearningEnrollmentsController(service);
        ControllerTestHelper.SetMalformedUserId(controller, "not-a-guid");

        var result = await controller.Enroll(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
        Assert.Null(service.LastOperation);
    }

    [Fact]
    public async Task Enroll_forwards_route_course_id_and_jwt_user_id_and_returns_created()
    {
        var service = new FakeEnrollmentService();
        var controller = new LearningEnrollmentsController(service);
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        ControllerTestHelper.SetUser(controller, userId);

        var result = await controller.Enroll(courseId, cts.Token);

        Assert.Equal(nameof(IEnrollmentService.EnrollAsync), service.LastOperation);
        Assert.Equal(courseId, service.LastCourseId);
        Assert.Equal(userId, service.LastUserId);
        Assert.Equal(cts.Token, service.LastCancellationToken);
        Assert.NotNull(service.LastEnrollRequest);
        Assert.Equal(courseId, service.LastEnrollRequest!.CourseId);
        Assert.Equal(userId, service.LastEnrollRequest.UserId);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(LearningEnrollmentsController.GetById), created.ActionName);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
    }

    [Fact]
    public void Enroll_action_has_no_request_body_parameter()
    {
        var method = typeof(LearningEnrollmentsController).GetMethod(nameof(LearningEnrollmentsController.Enroll));
        Assert.NotNull(method);

        Assert.DoesNotContain(
            method.GetParameters(),
            parameter => parameter.GetCustomAttribute<FromBodyAttribute>() is not null
                         || parameter.ParameterType == typeof(EnrollStudentRequest));
    }

    [Fact]
    public async Task ListMine_forwards_current_user_and_returns_ok()
    {
        var userId = Guid.NewGuid();
        var service = new FakeEnrollmentService
        {
            ListToReturn =
            [
                new EnrollmentListItemDto(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    userId,
                    DateTime.UtcNow,
                    "Active",
                    10),
            ],
        };
        var controller = new LearningEnrollmentsController(service);
        ControllerTestHelper.SetUser(controller, userId);

        var result = await controller.ListMine(CancellationToken.None);

        Assert.Equal(userId, service.LastUserId);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IReadOnlyList<EnrollmentListItemDto>>(ok.Value);
        Assert.All(items, item => Assert.Equal(userId, item.UserId));
    }

    [Fact]
    public async Task GetById_forwards_enrollment_id_and_current_user()
    {
        var service = new FakeEnrollmentService();
        var controller = new LearningEnrollmentsController(service);
        var userId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId);

        var result = await controller.GetById(enrollmentId, CancellationToken.None);

        Assert.Equal(enrollmentId, service.LastEnrollmentId);
        Assert.Equal(userId, service.LastUserId);
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByCourse_forwards_course_id_and_current_user()
    {
        var service = new FakeEnrollmentService();
        var controller = new LearningEnrollmentsController(service);
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId);

        var result = await controller.GetByCourse(courseId, CancellationToken.None);

        Assert.Equal(courseId, service.LastCourseId);
        Assert.Equal(userId, service.LastUserId);
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_maps_inaccessible_enrollment_to_not_found_via_exception()
    {
        var service = new FakeEnrollmentService
        {
            ExceptionToThrow = new EnrollmentException("Enrollment was not found.", EnrollmentErrorCodes.NotFound),
        };
        var controller = new LearningEnrollmentsController(service);
        ControllerTestHelper.SetUser(controller, Guid.NewGuid());

        var ex = await Assert.ThrowsAsync<EnrollmentException>(() =>
            controller.GetById(Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(EnrollmentErrorCodes.NotFound, ex.Code);
    }

    [Fact]
    public async Task StartLesson_forwards_route_ids_and_jwt_user_without_body()
    {
        var service = new FakeEnrollmentService();
        var controller = new LearningEnrollmentsController(service);
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId);

        var result = await controller.StartLesson(courseId, lessonId, CancellationToken.None);

        Assert.Equal(nameof(IEnrollmentService.StartLessonAsync), service.LastOperation);
        Assert.Equal(courseId, service.LastCourseId);
        Assert.Equal(lessonId, service.LastLessonId);
        Assert.Equal(userId, service.LastUserId);
        Assert.IsType<OkObjectResult>(result.Result);

        var method = typeof(LearningEnrollmentsController).GetMethod(nameof(LearningEnrollmentsController.StartLesson));
        Assert.DoesNotContain(
            method!.GetParameters(),
            parameter => parameter.GetCustomAttribute<FromBodyAttribute>() is not null);
    }

    [Fact]
    public async Task CompleteLesson_forwards_route_ids_and_jwt_user_without_progress_logic()
    {
        var service = new FakeEnrollmentService();
        var controller = new LearningEnrollmentsController(service);
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId);

        var result = await controller.CompleteLesson(courseId, lessonId, CancellationToken.None);

        Assert.Equal(nameof(IEnrollmentService.CompleteLessonAsync), service.LastOperation);
        Assert.Equal(courseId, service.LastCourseId);
        Assert.Equal(lessonId, service.LastLessonId);
        Assert.Equal(userId, service.LastUserId);
        Assert.IsType<OkObjectResult>(result.Result);

        var source = typeof(LearningEnrollmentsController)
            .GetMethod(nameof(LearningEnrollmentsController.CompleteLesson))!
            .DeclaringType!
            .FullName;
        Assert.Equal(typeof(LearningEnrollmentsController).FullName, source);
        Assert.DoesNotContain(
            typeof(LearningEnrollmentsController).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
            method => method.Name.Contains("Progress", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Controller_depends_only_on_IEnrollmentService()
    {
        var ctor = typeof(LearningEnrollmentsController).GetConstructors().Single();
        Assert.Single(ctor.GetParameters());
        Assert.Equal(typeof(IEnrollmentService), ctor.GetParameters()[0].ParameterType);
        Assert.DoesNotContain(
            ctor.GetParameters(),
            parameter => parameter.ParameterType.Name.Contains("Repository", StringComparison.Ordinal)
                         || parameter.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal)
                         || parameter.ParameterType == typeof(IDomainEventDispatcher));
    }
}

public sealed class EnrollmentExceptionFilterTests
{
    [Theory]
    [InlineData(EnrollmentErrorCodes.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(EnrollmentErrorCodes.CourseNotFound, StatusCodes.Status404NotFound)]
    [InlineData(EnrollmentErrorCodes.LessonNotInCourse, StatusCodes.Status404NotFound)]
    [InlineData(EnrollmentErrorCodes.AlreadyExists, StatusCodes.Status409Conflict)]
    [InlineData(EnrollmentErrorCodes.CourseNotPublished, StatusCodes.Status409Conflict)]
    [InlineData(EnrollmentErrorCodes.CourseHasNoLessons, StatusCodes.Status409Conflict)]
    [InlineData(EnrollmentErrorCodes.OperationInvalid, StatusCodes.Status409Conflict)]
    [InlineData(EnrollmentErrorCodes.UserInvalid, StatusCodes.Status400BadRequest)]
    [InlineData(EnrollmentErrorCodes.CourseInvalid, StatusCodes.Status400BadRequest)]
    [InlineData(EnrollmentErrorCodes.LessonInvalid, StatusCodes.Status400BadRequest)]
    [InlineData("enrollment_unknown", StatusCodes.Status400BadRequest)]
    public void Filter_maps_enrollment_exception_codes_to_status(string code, int expectedStatus)
    {
        var filter = new EnrollmentExceptionFilter();
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = new EnrollmentException("boom", code),
        };

        filter.OnException(context);

        Assert.True(context.ExceptionHandled);
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(expectedStatus, result.StatusCode);

        var json = System.Text.Json.JsonSerializer.Serialize(result.Value);
        Assert.Contains("\"message\"", json, StringComparison.Ordinal);
        Assert.Contains("\"code\"", json, StringComparison.Ordinal);
        Assert.Contains(code, json, StringComparison.Ordinal);
    }
}
