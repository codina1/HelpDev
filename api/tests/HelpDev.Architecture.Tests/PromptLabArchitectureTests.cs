using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using HelpDev.API.Controllers;
using HelpDev.Infrastructure.Outbox;
using HelpDev.Modules.PromptLab.Application.Catalog;
using HelpDev.Modules.PromptLab.Application.Prompts;
using HelpDev.Modules.PromptLab.Application.Rendering;
using HelpDev.Modules.PromptLab.Domain.Favorites;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.Modules.PromptLab.Presentation;
using HelpDev.SharedApplication.Abstractions.Events;
using Microsoft.EntityFrameworkCore;
using NetArchTest.Rules;
using PromptLabModuleMarker = HelpDev.Modules.PromptLab.ModuleMarker;
using AdministrationModuleMarker = HelpDev.Modules.Administration.ModuleMarker;
using ContentModuleMarker = HelpDev.Modules.Content.ModuleMarker;
using IdentityModuleMarker = HelpDev.Modules.Identity.ModuleMarker;
using LearningModuleMarker = HelpDev.Modules.Learning.ModuleMarker;
using SearchModuleMarker = HelpDev.Modules.Search.ModuleMarker;
using ToolboxModuleMarker = HelpDev.Modules.Toolbox.ModuleMarker;

namespace HelpDev.Architecture.Tests;

public sealed class PromptLabArchitectureTests
{
    [Fact]
    public void PromptLab_module_exposes_layered_namespaces_and_registration()
    {
        var assembly = typeof(PromptLabModuleMarker).Assembly;
        var namespaces = assembly.GetTypes().Select(type => type.Namespace ?? string.Empty).ToHashSet();

        Assert.Contains(namespaces, ns => ns.Contains(".Domain", StringComparison.Ordinal));
        Assert.Contains(namespaces, ns => ns.Contains(".Application", StringComparison.Ordinal));
        Assert.Contains(namespaces, ns => ns.Contains(".Infrastructure", StringComparison.Ordinal));
        Assert.Contains(namespaces, ns => ns.Contains(".Presentation", StringComparison.Ordinal));
        Assert.NotNull(typeof(PromptLabPresentationMarker));

        var registration = typeof(HelpDev.Modules.PromptLab.DependencyInjection)
            .GetMethod(nameof(HelpDev.Modules.PromptLab.DependencyInjection.AddPromptLabModule));
        Assert.NotNull(registration);
        Assert.True(registration!.IsStatic);
    }

    [Fact]
    public void PromptLab_Domain_does_not_depend_on_Application_Infrastructure_or_Presentation()
    {
        var result = Types.InAssembly(typeof(PromptLabModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "HelpDev.Modules.PromptLab.Application",
                "HelpDev.Modules.PromptLab.Infrastructure",
                "HelpDev.Modules.PromptLab.Presentation")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void PromptLab_Application_does_not_depend_on_Infrastructure_or_Presentation()
    {
        var result = Types.InAssembly(typeof(PromptLabModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application")
            .ShouldNot()
            .HaveDependencyOnAny(
                "HelpDev.Modules.PromptLab.Infrastructure",
                "HelpDev.Modules.PromptLab.Presentation")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void PromptLab_Presentation_does_not_depend_on_Infrastructure()
    {
        var result = Types.InAssembly(typeof(PromptLabModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Presentation")
            .ShouldNot()
            .HaveDependencyOnAny(
                "HelpDev.Modules.PromptLab.Infrastructure",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void PromptLab_Domain_depends_only_on_allowed_building_blocks()
    {
        var result = Types.InAssembly(typeof(PromptLabModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "HelpDev.Modules.Identity",
                "HelpDev.Modules.Content",
                "HelpDev.Modules.Learning",
                "HelpDev.Modules.Search",
                "HelpDev.Modules.Administration",
                "HelpDev.Modules.Toolbox",
                "HelpDev.Infrastructure",
                "Microsoft.AspNetCore",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void PromptLab_Application_services_do_not_depend_on_AspNetCore_EF_or_other_module_Infrastructure()
    {
        foreach (var ns in new[]
                 {
                     ".Application.Catalog",
                     ".Application.Categories",
                     ".Application.Prompts",
                     ".Application.Rendering",
                     ".Application.Favorites",
                     ".Application.History",
                 })
        {
            var result = Types.InAssembly(typeof(PromptLabModuleMarker).Assembly)
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
                    "HelpDev.Modules.Toolbox.Infrastructure",
                    "HelpDev.Infrastructure")
                .GetResult();

            Assert.True(result.IsSuccessful, $"{ns}: {FormatFailures(result)}");
        }
    }

    [Fact]
    public void Other_modules_do_not_depend_on_PromptLab()
    {
        AssertNoDependency(typeof(IdentityModuleMarker).Assembly, "HelpDev.Modules.PromptLab");
        AssertNoDependency(typeof(ContentModuleMarker).Assembly, "HelpDev.Modules.PromptLab");
        AssertNoDependency(typeof(LearningModuleMarker).Assembly, "HelpDev.Modules.PromptLab");
        // Search may reference PromptLab domain events for Outbox semantic indexing (same pattern as Content/Learning).
        AssertNoDependency(typeof(AdministrationModuleMarker).Assembly, "HelpDev.Modules.PromptLab");
        AssertNoDependency(typeof(ToolboxModuleMarker).Assembly, "HelpDev.Modules.PromptLab");
    }

    [Fact]
    public void PromptLab_API_controllers_depend_on_abstractions_not_Infrastructure()
    {
        var result = Types.InAssembly(typeof(PromptLabCatalogController).Assembly)
            .That()
            .HaveNameStartingWith("PromptLab")
            .ShouldNot()
            .HaveDependencyOnAny(
                "HelpDev.Modules.PromptLab.Infrastructure",
                typeof(IDomainEventDispatcher).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Catalog_depends_on_render_catalog_and_public_queries_not_DbContext()
    {
        var ctor = typeof(PromptLabCatalogController).GetConstructors().Single();
        Assert.Contains(ctor.GetParameters(), p => p.ParameterType == typeof(IPromptRenderService));
        Assert.Contains(ctor.GetParameters(), p => p.ParameterType == typeof(IPromptCatalogQueries));
        Assert.Contains(ctor.GetParameters(), p => p.ParameterType == typeof(IPromptPublicQueries));
        Assert.DoesNotContain(
            ctor.GetParameters(),
            p => typeof(DbContext).IsAssignableFrom(p.ParameterType)
                || p.ParameterType.Name.Contains("Repository", StringComparison.Ordinal));
    }

    [Fact]
    public void Writer_depends_on_writer_service_and_queries_not_DbContext()
    {
        var ctor = typeof(PromptLabWriterController).GetConstructors().Single();
        Assert.Contains(ctor.GetParameters(), p => p.ParameterType == typeof(IPromptWriterService));
        Assert.Contains(ctor.GetParameters(), p => p.ParameterType == typeof(IPromptWriterQueries));
        Assert.DoesNotContain(
            ctor.GetParameters(),
            p => typeof(DbContext).IsAssignableFrom(p.ParameterType)
                || p.ParameterType.Name.Contains("Repository", StringComparison.Ordinal));
    }

    [Fact]
    public void Renderer_and_parser_do_not_depend_on_DbContext_HttpClient_or_Process()
    {
        var result = Types.InAssembly(typeof(PromptLabModuleMarker).Assembly)
            .That()
            .HaveNameMatching("PromptRenderer|PromptTemplateParser")
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
    public void PromptLab_has_no_OpenAI_Anthropic_Scriban_Liquid_or_Razor_package_dependency()
    {
        var assembly = typeof(PromptLabModuleMarker).Assembly;
        var referenced = assembly.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty).ToList();

        Assert.DoesNotContain(referenced, name => name.Contains("OpenAI", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referenced, name => name.Contains("Anthropic", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referenced, name => name.Contains("Scriban", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referenced, name => name.Contains("Fluid", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referenced, name => name.Contains("Liquid", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referenced, name => name.Contains("Razor", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PromptLab_application_does_not_write_OutboxMessage_or_dispatch_events()
    {
        var result = Types.InAssembly(typeof(PromptLabModuleMarker).Assembly)
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
    public void PromptVersion_has_no_public_mutation_methods()
    {
        var methods = typeof(PromptVersion)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(method => !method.Name.StartsWith("get_", StringComparison.Ordinal))
            .Select(method => method.Name)
            .ToList();

        Assert.DoesNotContain(methods, name => name.Contains("Update", StringComparison.Ordinal));
        Assert.DoesNotContain(
            methods,
            name => name.StartsWith("Set", StringComparison.Ordinal)
                && !name.StartsWith("get_", StringComparison.Ordinal));
    }

    [Fact]
    public void Favorites_use_scalar_UserId_and_no_Identity_User_reference()
    {
        Assert.Equal(typeof(Guid), typeof(PromptFavorite).GetProperty("UserId")!.PropertyType);
        var result = Types.InAssembly(typeof(PromptLabModuleMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Modules.Identity.Domain.Entities.User")
            .GetResult();
        Assert.True(result.IsSuccessful, FormatFailures(result));
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
