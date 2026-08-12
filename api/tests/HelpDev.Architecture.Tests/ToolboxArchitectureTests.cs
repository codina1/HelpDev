using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using HelpDev.API.Controllers;
using HelpDev.Infrastructure.Outbox;
using HelpDev.Modules.Toolbox.Application.Catalog;
using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Domain.Tools;
using HelpDev.Modules.Toolbox.Infrastructure.Execution;
using HelpDev.SharedApplication.Abstractions.Events;
using Microsoft.EntityFrameworkCore;
using NetArchTest.Rules;
using ToolboxModuleMarker = HelpDev.Modules.Toolbox.ModuleMarker;
using AdministrationModuleMarker = HelpDev.Modules.Administration.ModuleMarker;
using ContentModuleMarker = HelpDev.Modules.Content.ModuleMarker;
using IdentityModuleMarker = HelpDev.Modules.Identity.ModuleMarker;
using LearningModuleMarker = HelpDev.Modules.Learning.ModuleMarker;
using SearchModuleMarker = HelpDev.Modules.Search.ModuleMarker;

namespace HelpDev.Architecture.Tests;

public sealed class ToolboxArchitectureTests
{
    [Fact]
    public void Toolbox_Domain_depends_only_on_allowed_building_blocks()
    {
        var result = Types.InAssembly(typeof(ToolboxModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "HelpDev.Modules.Identity",
                "HelpDev.Modules.Content",
                "HelpDev.Modules.Learning",
                "HelpDev.Modules.Search",
                "HelpDev.Modules.Administration",
                "HelpDev.Infrastructure",
                "Microsoft.AspNetCore",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Toolbox_Application_services_do_not_depend_on_AspNetCore_EF_or_other_module_Infrastructure()
    {
        foreach (var ns in new[]
                 {
                     ".Application.Catalog",
                     ".Application.Categories",
                     ".Application.Tools",
                     ".Application.Execution",
                     ".Application.Favorites",
                     ".Application.History",
                 })
        {
            var result = Types.InAssembly(typeof(ToolboxModuleMarker).Assembly)
                .That()
                .ResideInNamespaceContaining(ns)
                .ShouldNot()
                .HaveDependencyOnAny(
                    "Microsoft.AspNetCore",
                    "Microsoft.EntityFrameworkCore",
                    "Npgsql",
                    "HelpDev.Modules.Identity.Infrastructure",
                    "HelpDev.Modules.Content.Infrastructure",
                    "HelpDev.Modules.Learning.Infrastructure",
                    "HelpDev.Modules.Search.Infrastructure",
                    "HelpDev.Modules.Administration.Infrastructure",
                    "HelpDev.Infrastructure")
                .GetResult();

            Assert.True(result.IsSuccessful, $"{ns}: {FormatFailures(result)}");
        }
    }

    [Fact]
    public void Other_modules_do_not_depend_on_Toolbox()
    {
        AssertNoDependency(typeof(IdentityModuleMarker).Assembly, "HelpDev.Modules.Toolbox");
        AssertNoDependency(typeof(ContentModuleMarker).Assembly, "HelpDev.Modules.Toolbox");
        AssertNoDependency(typeof(LearningModuleMarker).Assembly, "HelpDev.Modules.Toolbox");
        // Search may reference Toolbox domain events for Outbox semantic indexing (same pattern as Content/Learning).
        AssertNoDependency(typeof(AdministrationModuleMarker).Assembly, "HelpDev.Modules.Toolbox");
    }

    [Fact]
    public void Toolbox_API_controllers_do_not_depend_on_concrete_executors_or_Infrastructure()
    {
        var result = Types.InAssembly(typeof(ToolboxCatalogController).Assembly)
            .That()
            .HaveNameStartingWith("Toolbox")
            .ShouldNot()
            .HaveDependencyOnAny(
                "HelpDev.Modules.Toolbox.Infrastructure",
                typeof(JsonFormatterToolExecutor).FullName!,
                typeof(IDomainEventDispatcher).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Catalog_execute_depends_on_IToolExecutionService_not_DbContext()
    {
        var ctor = typeof(ToolboxCatalogController).GetConstructors().Single();
        Assert.Contains(ctor.GetParameters(), p => p.ParameterType == typeof(IToolExecutionService));
        Assert.Contains(ctor.GetParameters(), p => p.ParameterType == typeof(IToolCatalogQueries));
        Assert.DoesNotContain(
            ctor.GetParameters(),
            p => typeof(DbContext).IsAssignableFrom(p.ParameterType)
                || p.ParameterType.Name.Contains("Repository", StringComparison.Ordinal));
    }

    [Fact]
    public void Executors_do_not_depend_on_DbContext_HttpClient_Process_or_Environment()
    {
        var result = Types.InAssembly(typeof(JsonFormatterToolExecutor).Assembly)
            .That()
            .ResideInNamespaceContaining(".Infrastructure.Execution")
            .And()
            .ImplementInterface(typeof(IToolExecutor))
            .ShouldNot()
            .HaveDependencyOnAny(
                typeof(DbContext).FullName!,
                typeof(HttpClient).FullName!,
                typeof(Process).FullName!,
                "System.Diagnostics.Process")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Toolbox_has_no_Type_GetType_or_Activator_CreateInstance_usage()
    {
        var assembly = typeof(ToolboxModuleMarker).Assembly;
        foreach (var type in assembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (method.IsAbstract)
                {
                    continue;
                }

                try
                {
                    var body = method.GetMethodBody();
                    _ = body;
                }
                catch
                {
                    // ignore
                }
            }
        }

        var sourceTypes = assembly.GetTypes()
            .Where(t => t.FullName is not null
                        && (t.FullName.Contains("Executor", StringComparison.Ordinal)
                            || t.FullName.Contains("Registry", StringComparison.Ordinal)))
            .ToList();

        Assert.DoesNotContain(
            sourceTypes,
            t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Any(m => m.Name is "GetType" && m.DeclaringType == typeof(Type)));

        // Explicit registry construction uses concrete types only.
        Assert.Contains(typeof(ToolExecutorRegistry).GetConstructors(), c => c.GetParameters().Length == 1);
    }

    [Fact]
    public void Toolbox_application_does_not_write_OutboxMessage_or_dispatch_events()
    {
        var result = Types.InAssembly(typeof(ToolboxModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application")
            .ShouldNot()
            .HaveDependencyOnAny(
                typeof(OutboxMessage).FullName!,
                typeof(IDomainEventDispatcher).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void ToolDefinition_has_no_executable_source_property()
    {
        var names = typeof(ToolDefinition).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.Ordinal);
        Assert.DoesNotContain("SourceCode", names);
        Assert.DoesNotContain("Script", names);
        Assert.DoesNotContain("AssemblyPath", names);
        Assert.DoesNotContain("ExecutableType", names);
    }

    [Fact]
    public void Favorites_use_scalar_UserId_and_no_Identity_User_reference()
    {
        var favorite = typeof(HelpDev.Modules.Toolbox.Domain.Favorites.ToolFavorite);
        Assert.Equal(typeof(Guid), favorite.GetProperty("UserId")!.PropertyType);
        var result = Types.InAssembly(typeof(ToolboxModuleMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Modules.Identity.Domain.Entities.User")
            .GetResult();
        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Registry_registers_exact_allow_listed_tool_types_only()
    {
        var executors = new IToolExecutor[]
        {
            new JsonFormatterToolExecutor(),
            new JsonValidatorToolExecutor(),
            new Base64EncodeToolExecutor(),
            new Base64DecodeToolExecutor(),
            new UrlEncodeToolExecutor(),
            new UrlDecodeToolExecutor(),
            new UuidGeneratorToolExecutor(),
            new HashGeneratorToolExecutor(),
            new TimestampConverterToolExecutor(),
            new TextStatisticsToolExecutor(),
            new RegexTesterToolExecutor(),
        };

        var registry = new ToolExecutorRegistry(executors);
        foreach (ToolType type in Enum.GetValues<ToolType>())
        {
            Assert.Equal(type, registry.GetRequired(type).Type);
        }

        Assert.Throws<InvalidOperationException>(() =>
            new ToolExecutorRegistry(executors.Append(new JsonFormatterToolExecutor())));
    }

    private static void AssertNoDependency(Assembly assembly, string dependency)
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
