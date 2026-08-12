using HelpDev.API.Controllers;
using HelpDev.API.Filters;
using HelpDev.API.Tests.Fakes;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Domain.Courses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace HelpDev.API.Tests;

public sealed class LearningCoursesControllerTests
{
    [Fact]
    public async Task List_returns_published_courses_only_from_public_queries()
    {
        var queries = new FakePublicCourseQueries
        {
            PublishedList =
            [
                new CourseListItemDto(
                    Guid.NewGuid(),
                    "Published",
                    "published",
                    nameof(CourseStatus.Published),
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    1,
                    2),
            ],
        };
        var controller = new LearningCoursesController(queries);

        var result = await controller.List(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IReadOnlyList<CourseListItemDto>>(ok.Value);
        Assert.All(items, item => Assert.Equal(nameof(CourseStatus.Published), item.Status));
    }

    [Fact]
    public async Task GetById_returns_not_found_for_missing_or_draft()
    {
        var queries = new FakePublicCourseQueries { PublishedById = null };
        var controller = new LearningCoursesController(queries);

        var ex = await Assert.ThrowsAsync<CourseException>(() =>
            controller.GetById(Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(CourseErrorCodes.NotFound, ex.Code);
    }

    [Fact]
    public async Task GetBySlug_forwards_slug_to_queries()
    {
        var courseId = Guid.NewGuid();
        var queries = new FakePublicCourseQueries
        {
            PublishedBySlug = FakeCourseService.CreateSampleDetail() with
            {
                Id = courseId,
                Status = nameof(CourseStatus.Published),
                Slug = "intro-csharp",
            },
        };
        var controller = new LearningCoursesController(queries);

        var result = await controller.GetBySlug("intro-csharp", CancellationToken.None);

        Assert.Equal("intro-csharp", queries.LastGetBySlug);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CourseDetailDto>(ok.Value);
        Assert.Equal(courseId, dto.Id);
        Assert.Equal(nameof(CourseStatus.Published), dto.Status);
    }
}

public sealed class LearningCourseManagementControllerTests
{
    [Fact]
    public async Task Create_uses_authenticated_user_as_instructor_and_ignores_body_instructor()
    {
        var service = new FakeCourseService();
        var controller = new LearningCourseManagementController(service);
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId, AppRoles.Writer);

        var request = new CreateCourseRequest
        {
            Title = "Course",
            Slug = "course",
            Description = "Desc",
        };

        var result = await controller.Create(request, CancellationToken.None);

        Assert.Equal(userId, service.LastInstructorId);
        Assert.NotNull(service.LastActor);
        Assert.False(service.LastActor!.CanManageAllCourses);
        Assert.NotNull(service.LastCreateRequest);
        Assert.Null(typeof(CreateCourseRequest).GetProperty("InstructorId"));
        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task Create_returns_unauthorized_when_current_user_missing()
    {
        var service = new FakeCourseService();
        var controller = new LearningCourseManagementController(service);
        ControllerTestHelper.SetUser(controller, userId: null);

        var result = await controller.Create(
            new CreateCourseRequest { Title = "T", Slug = "t" },
            CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
        Assert.Null(service.LastActor);
    }

    [Fact]
    public async Task Malformed_user_id_returns_unauthorized()
    {
        var service = new FakeCourseService();
        var controller = new LearningCourseManagementController(service);
        ControllerTestHelper.SetMalformedUserId(controller, "bad-id");

        var result = await controller.List(null, CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
        Assert.Null(service.LastActor);
    }

    [Fact]
    public async Task Writer_actor_has_CanManageAllCourses_false()
    {
        var service = new FakeCourseService();
        var controller = new LearningCourseManagementController(service);
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId, AppRoles.Writer);

        await controller.List(null, CancellationToken.None);

        Assert.Equal(userId, service.LastActor!.UserId);
        Assert.False(service.LastActor.CanManageAllCourses);
    }

    [Fact]
    public async Task Admin_actor_has_CanManageAllCourses_true()
    {
        var service = new FakeCourseService();
        var controller = new LearningCourseManagementController(service);
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId, AppRoles.Admin);

        await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(userId, service.LastActor!.UserId);
        Assert.True(service.LastActor.CanManageAllCourses);
    }

    [Fact]
    public void Management_controller_requires_writer_or_admin_policy()
    {
        var attribute = Assert.Single(
            typeof(LearningCourseManagementController)
                .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), inherit: true)
                .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>());

        Assert.Equal(AuthorizationPolicies.WriterOrAdmin, attribute.Policy);
    }

    [Fact]
    public async Task Update_and_publish_forward_route_course_id_and_actor()
    {
        var service = new FakeCourseService();
        var controller = new LearningCourseManagementController(service);
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId, AppRoles.Writer);
        var courseId = Guid.NewGuid();

        await controller.UpdateDetails(
            courseId,
            new UpdateCourseRequest { Title = "T", Slug = "t", Description = "D" },
            CancellationToken.None);
        Assert.Equal(courseId, service.LastCourseId);
        Assert.Equal(userId, service.LastActor!.UserId);
        Assert.Equal(nameof(ICourseService.UpdateDetailsAsync), service.LastOperation);

        await controller.Publish(courseId, CancellationToken.None);
        Assert.Equal(courseId, service.LastCourseId);
        Assert.Equal(nameof(ICourseService.PublishAsync), service.LastOperation);
    }

    [Fact]
    public void UpdateDetails_route_contract_remains_unchanged()
    {
        var method = typeof(LearningCourseManagementController).GetMethod(
            nameof(LearningCourseManagementController.UpdateDetails));
        Assert.NotNull(method);

        var httpPut = method!.GetCustomAttributes(typeof(HttpPutAttribute), inherit: true)
            .Cast<HttpPutAttribute>()
            .Single();
        Assert.Equal("{id:guid}", httpPut.Template);

        var parameters = method.GetParameters();
        Assert.Equal("id", parameters[0].Name);
        Assert.Equal(typeof(Guid), parameters[0].ParameterType);
        Assert.Equal(typeof(UpdateCourseRequest), parameters[1].ParameterType);
        Assert.Null(typeof(UpdateCourseRequest).GetProperty("InstructorId"));
        Assert.Empty(method.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute), inherit: true));
    }

    [Fact]
    public async Task Section_and_lesson_actions_forward_route_identifiers_and_actor()
    {
        var service = new FakeCourseService();
        var controller = new LearningCourseManagementController(service);
        var userId = Guid.NewGuid();
        ControllerTestHelper.SetUser(controller, userId, AppRoles.Admin);
        var courseId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();

        await controller.RenameSection(
            courseId,
            sectionId,
            new RenameSectionBody { Title = "Renamed" },
            CancellationToken.None);
        Assert.Equal(courseId, service.LastCourseId);
        Assert.Equal(sectionId, service.LastSectionId);
        Assert.True(service.LastActor!.CanManageAllCourses);

        await controller.ReorderSection(
            courseId,
            sectionId,
            new ReorderBody { NewOrder = 2 },
            CancellationToken.None);
        Assert.Equal(sectionId, service.LastSectionId);

        await controller.AddLesson(
            courseId,
            sectionId,
            new AddLessonBody { Title = "Lesson" },
            CancellationToken.None);
        Assert.Equal(sectionId, service.LastSectionId);

        await controller.UpdateLesson(
            courseId,
            sectionId,
            lessonId,
            new UpdateLessonBody { Title = "Updated" },
            CancellationToken.None);
        Assert.Equal(lessonId, service.LastLessonId);

        await controller.ReorderLesson(
            courseId,
            sectionId,
            lessonId,
            new ReorderBody { NewOrder = 1 },
            CancellationToken.None);
        Assert.Equal(lessonId, service.LastLessonId);
    }

    [Fact]
    public void Controllers_depend_on_application_interfaces_not_repositories()
    {
        var publicCtor = typeof(LearningCoursesController).GetConstructors().Single();
        Assert.Contains(publicCtor.GetParameters(), p => p.ParameterType == typeof(IPublicCourseQueries));
        Assert.DoesNotContain(
            publicCtor.GetParameters(),
            p => p.ParameterType.Name.Contains("Repository", StringComparison.Ordinal)
                 || p.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal));

        var manageCtor = typeof(LearningCourseManagementController).GetConstructors().Single();
        Assert.Contains(manageCtor.GetParameters(), p => p.ParameterType == typeof(ICourseService));
        Assert.DoesNotContain(
            manageCtor.GetParameters(),
            p => p.ParameterType.Name.Contains("Repository", StringComparison.Ordinal)
                 || p.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal));
    }

    [Fact]
    public void Request_contracts_do_not_expose_ownership_flags_or_instructor_id()
    {
        Assert.Null(typeof(CreateCourseRequest).GetProperty("InstructorId"));
        Assert.Null(typeof(CreateCourseRequest).GetProperty("CanManageAllCourses"));
        Assert.Null(typeof(UpdateCourseRequest).GetProperty("InstructorId"));
        Assert.Null(typeof(UpdateCourseRequest).GetProperty("CanManageAllCourses"));
    }
}

public sealed class CourseExceptionFilterTests
{
    [Theory]
    [InlineData(CourseErrorCodes.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(CourseErrorCodes.SlugDuplicate, StatusCodes.Status409Conflict)]
    [InlineData(CourseErrorCodes.OperationInvalid, StatusCodes.Status409Conflict)]
    [InlineData(CourseErrorCodes.SlugInvalid, StatusCodes.Status400BadRequest)]
    public void Filter_maps_course_exception_codes_to_status(string code, int expectedStatus)
    {
        var filter = new CourseExceptionFilter();
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = new CourseException("boom", code),
        };

        filter.OnException(context);

        Assert.True(context.ExceptionHandled);
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(expectedStatus, result.StatusCode);
    }
}
