using System.Reflection;
using HelpDev.API.Controllers;
using HelpDev.Infrastructure.Outbox;
using HelpDev.Infrastructure.Persistence;
using HelpDev.Infrastructure.Search;
using HelpDev.Modules.Content.Domain.Entities;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.Search.Application.Dtos;
using HelpDev.Modules.Search.Application.Handlers;
using HelpDev.Modules.Search.Application.Persistence;
using HelpDev.Modules.Search.Application.Reindex;
using HelpDev.Modules.Search.Domain;
using HelpDev.SharedInfrastructure.Outbox;
using NetArchTest.Rules;
using ContentModuleMarker = HelpDev.Modules.Content.ModuleMarker;
using LearningModuleMarker = HelpDev.Modules.Learning.ModuleMarker;
using SearchModuleMarker = HelpDev.Modules.Search.ModuleMarker;

namespace HelpDev.Architecture.Tests;

public sealed class SearchArchitectureTests
{
    [Fact]
    public void Search_Application_does_not_depend_on_Content_or_Learning_Infrastructure()
    {
        var result = Types.InAssembly(typeof(SearchModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application")
            .ShouldNot()
            .HaveDependencyOnAny(
                "HelpDev.Modules.Content.Infrastructure",
                "HelpDev.Modules.Learning.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Search_does_not_reference_Content_or_Course_domain_entities()
    {
        var result = Types.InAssembly(typeof(SearchModuleMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                typeof(Content).FullName!,
                typeof(Course).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Content_and_Learning_do_not_depend_on_Search()
    {
        AssertNoDependency(typeof(ContentModuleMarker).Assembly, "HelpDev.Modules.Search");
        AssertNoDependency(typeof(LearningModuleMarker).Assembly, "HelpDev.Modules.Search");
    }

    [Fact]
    public void Api_Search_controller_does_not_depend_on_Search_Infrastructure()
    {
        var result = Types.InAssembly(typeof(SearchController).Assembly)
            .That()
            .HaveName(nameof(SearchController))
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Modules.Search.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Search_handlers_do_not_depend_on_ApplicationDbContext()
    {
        var result = Types.InAssembly(typeof(ContentPublishedSearchHandler).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application.Handlers")
            .ShouldNot()
            .HaveDependencyOn(typeof(ApplicationDbContext).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Search_API_DTOs_expose_no_Domain_or_EF_types()
    {
        foreach (var dtoType in new[] { typeof(SearchResultDto), typeof(SearchItemDto) })
        {
            Assert.All(
                dtoType.GetProperties(BindingFlags.Instance | BindingFlags.Public),
                property =>
                {
                    Assert.False(
                        property.PropertyType.FullName?.StartsWith("HelpDev.Modules.", StringComparison.Ordinal) == true
                        && property.PropertyType.Namespace?.Contains(".Domain", StringComparison.Ordinal) == true);
                    Assert.DoesNotContain("EntityFrameworkCore", property.PropertyType.FullName ?? string.Empty);
                });
        }

        Assert.Null(typeof(SearchItemDto).GetProperty(nameof(SearchDocument.LastEventId)));
        Assert.Null(typeof(SearchItemDto).GetProperty(nameof(SearchDocument.IndexedAtUtc)));
    }

    [Fact]
    public void SharedInfrastructure_does_not_reference_Search()
    {
        var result = Types.InAssembly(typeof(IOutboxEventSerializer).Assembly)
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Modules.Search")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Search_does_not_manually_modify_OutboxMessage()
    {
        var result = Types.InAssembly(typeof(SearchModuleMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn(typeof(OutboxMessage).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Search_module_does_not_depend_on_legacy_host_Infrastructure_from_Application()
    {
        var result = Types.InAssembly(typeof(SearchModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application")
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Search_handlers_depend_on_ISearchDbContext_only_through_repository_ports()
    {
        var handlerTypes = Types.InAssembly(typeof(ContentPublishedSearchHandler).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application.Handlers")
            .And()
            .ImplementInterface(typeof(HelpDev.SharedApplication.Abstractions.Events.IDomainEventHandler<>))
            .GetTypes();

        Assert.All(handlerTypes, type =>
        {
            var ctor = type.GetConstructors().Single();
            Assert.DoesNotContain(
                ctor.GetParameters(),
                parameter => parameter.ParameterType == typeof(ApplicationDbContext)
                    || parameter.ParameterType == typeof(ISearchDbContext));
        });
    }

    [Fact]
    public void CourseUpdatedDomainEvent_does_not_depend_on_Search()
    {
        var result = Types.InAssembly(typeof(CourseUpdatedDomainEvent).Assembly)
            .That()
            .HaveName(nameof(CourseUpdatedDomainEvent))
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Modules.Search")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void CourseUpdatedSearchHandler_does_not_access_Learning_Infrastructure()
    {
        var result = Types.InAssembly(typeof(CourseUpdatedSearchHandler).Assembly)
            .That()
            .HaveName(nameof(CourseUpdatedSearchHandler))
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Modules.Learning.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Api_course_management_controller_does_not_depend_on_CourseUpdatedSearchHandler()
    {
        var result = Types.InAssembly(typeof(LearningCourseManagementController).Assembly)
            .That()
            .HaveName(nameof(LearningCourseManagementController))
            .ShouldNot()
            .HaveDependencyOn(typeof(CourseUpdatedSearchHandler).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Learning_Application_does_not_depend_on_Outbox_serializer_or_store()
    {
        var result = Types.InAssembly(typeof(LearningModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application")
            .ShouldNot()
            .HaveDependencyOnAny(
                typeof(IOutboxEventSerializer).FullName!,
                typeof(IOutboxMessageStore).FullName!,
                typeof(OutboxMessage).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Search_reindex_service_does_not_depend_on_AspNetCore()
    {
        var result = Types.InAssembly(typeof(SearchReindexService).Assembly)
            .That()
            .HaveName(nameof(SearchReindexService))
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Search_Application_does_not_depend_on_Postgres_reindex_lock()
    {
        var result = Types.InAssembly(typeof(SearchModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application")
            .ShouldNot()
            .HaveDependencyOn(typeof(PostgresSearchReindexLock).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Api_SearchManage_controller_does_not_depend_on_Search_Infrastructure()
    {
        var result = Types.InAssembly(typeof(SearchManageController).Assembly)
            .That()
            .HaveName(nameof(SearchManageController))
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Modules.Search.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Search_reindex_does_not_depend_on_IDomainEventDispatcher()
    {
        var result = Types.InAssembly(typeof(SearchReindexService).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application.Reindex")
            .ShouldNot()
            .HaveDependencyOn(typeof(HelpDev.SharedApplication.Abstractions.Events.IDomainEventDispatcher).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Content_and_Learning_do_not_depend_on_Search_reindex()
    {
        AssertNoDependency(typeof(ContentModuleMarker).Assembly, typeof(SearchReindexService).FullName!);
        AssertNoDependency(typeof(LearningModuleMarker).Assembly, typeof(ISearchReindexService).FullName!);
    }

    [Fact]
    public void Content_Application_InternalLinks_does_not_depend_on_Search_module()
    {
        var result = Types.InAssembly(typeof(ContentModuleMarker).Assembly)
            .That()
            .ResideInNamespaceStartingWith("HelpDev.Modules.Content.Application.InternalLinks")
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Modules.Search")
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

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}
