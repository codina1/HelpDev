using System.Reflection;
using HelpDev.API.Controllers;
using HelpDev.Infrastructure.Administration;
using HelpDev.Infrastructure.Outbox;
using HelpDev.Infrastructure.Persistence;
using HelpDev.Modules.Administration.Application.Dashboard;
using HelpDev.Modules.Administration.Application.FeatureFlags;
using HelpDev.Modules.Administration.Application.Settings;
using HelpDev.Modules.Administration.Domain.Announcements;
using HelpDev.Modules.Administration.Domain.FeatureFlags;
using HelpDev.Modules.Administration.Domain.Settings;
using HelpDev.SharedApplication.Abstractions.Events;
using Microsoft.EntityFrameworkCore;
using NetArchTest.Rules;
using AdministrationModuleMarker = HelpDev.Modules.Administration.ModuleMarker;
using ContentModuleMarker = HelpDev.Modules.Content.ModuleMarker;
using IdentityModuleMarker = HelpDev.Modules.Identity.ModuleMarker;
using LearningModuleMarker = HelpDev.Modules.Learning.ModuleMarker;
using SearchModuleMarker = HelpDev.Modules.Search.ModuleMarker;

namespace HelpDev.Architecture.Tests;

public sealed class AdministrationArchitectureTests
{
    [Fact]
    public void Administration_Domain_depends_only_on_SharedKernel()
    {
        var result = Types.InAssembly(typeof(AdministrationModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "HelpDev.Modules.Identity",
                "HelpDev.Modules.Content",
                "HelpDev.Modules.Learning",
                "HelpDev.Modules.Search",
                "HelpDev.Infrastructure",
                "Microsoft.AspNetCore",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Administration_Application_services_do_not_depend_on_AspNetCore_EF_or_other_module_Infrastructure()
    {
        foreach (var ns in new[]
                 {
                     ".Application.FeatureFlags",
                     ".Application.Settings",
                     ".Application.Announcements",
                     ".Application.Dashboard",
                 })
        {
            var result = Types.InAssembly(typeof(AdministrationModuleMarker).Assembly)
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
                    "HelpDev.Infrastructure")
                .GetResult();

            Assert.True(result.IsSuccessful, $"{ns}: {FormatFailures(result)}");
        }
    }

    [Fact]
    public void Other_modules_do_not_depend_on_Administration()
    {
        AssertNoDependency(typeof(IdentityModuleMarker).Assembly, "HelpDev.Modules.Administration");
        AssertNoDependency(typeof(ContentModuleMarker).Assembly, "HelpDev.Modules.Administration");
        AssertNoDependency(typeof(LearningModuleMarker).Assembly, "HelpDev.Modules.Administration");
        AssertNoDependency(typeof(SearchModuleMarker).Assembly, "HelpDev.Modules.Administration");
    }

    [Fact]
    public void Admin_API_controllers_do_not_depend_on_Administration_Infrastructure()
    {
        var result = Types.InAssembly(typeof(AdministrationDashboardController).Assembly)
            .That()
            .HaveNameStartingWith("Administration")
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Modules.Administration.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Dashboard_controller_depends_only_on_IAdministrationDashboardQueries()
    {
        var ctor = typeof(AdministrationDashboardController).GetConstructors().Single();
        Assert.Single(ctor.GetParameters());
        Assert.Equal(typeof(IAdministrationDashboardQueries), ctor.GetParameters()[0].ParameterType);
    }

    [Fact]
    public void Cross_module_dashboard_ports_expose_dtos_not_entities()
    {
        foreach (var type in new[]
                 {
                     typeof(IdentityAdministrationStatistics),
                     typeof(ContentAdministrationStatistics),
                     typeof(LearningAdministrationStatistics),
                     typeof(SearchAdministrationStatistics),
                     typeof(OutboxAdministrationStatistics),
                     typeof(AdministrationDashboardDto),
                     typeof(FeatureFlagDto),
                     typeof(SystemSettingDto),
                 })
        {
            Assert.All(
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public),
                property =>
                {
                    Assert.NotEqual(typeof(FeatureFlag), property.PropertyType);
                    Assert.NotEqual(typeof(SystemSetting), property.PropertyType);
                    Assert.NotEqual(typeof(Announcement), property.PropertyType);
                    Assert.NotEqual(typeof(ApplicationDbContext), property.PropertyType);
                    Assert.False(typeof(IQueryable).IsAssignableFrom(property.PropertyType));
                    Assert.False(typeof(DbContext).IsAssignableFrom(property.PropertyType));
                });
        }
    }

    [Fact]
    public void Administration_controllers_do_not_depend_on_IDomainEventDispatcher()
    {
        var result = Types.InAssembly(typeof(AdministrationDashboardController).Assembly)
            .That()
            .HaveNameStartingWith("Administration")
            .ShouldNot()
            .HaveDependencyOn(typeof(IDomainEventDispatcher).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Administration_services_do_not_write_OutboxMessage_directly()
    {
        var result = Types.InAssembly(typeof(AdministrationModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application")
            .ShouldNot()
            .HaveDependencyOn(typeof(OutboxMessage).FullName!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void FeatureFlag_and_SystemSetting_remain_separate_aggregates()
    {
        Assert.NotEqual(typeof(FeatureFlag), typeof(SystemSetting));
        Assert.True(typeof(FeatureFlag).IsSubclassOf(typeof(HelpDev.SharedKernel.Common.AggregateRoot<Guid>)));
        Assert.True(typeof(SystemSetting).IsSubclassOf(typeof(HelpDev.SharedKernel.Common.AggregateRoot<Guid>)));
    }

    [Fact]
    public void Administration_module_does_not_introduce_user_or_role_crud_types()
    {
        var typeNames = typeof(AdministrationModuleMarker).Assembly.GetTypes().Select(type => type.Name);
        Assert.DoesNotContain(typeNames, name => name.Contains("UserRole", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(typeNames, name => name.Contains("Permission", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(typeNames, name => name is "AdminUserService" or "IAdminUserService");
    }

    [Fact]
    public void Dashboard_adapters_live_in_composition_root_not_Administration_Infrastructure()
    {
        Assert.Equal("HelpDev.Infrastructure", typeof(IdentityAdministrationStatisticsSource).Assembly.GetName().Name);
        Assert.Equal("HelpDev.Infrastructure", typeof(ContentAdministrationStatisticsSource).Assembly.GetName().Name);
        Assert.Equal("HelpDev.Infrastructure", typeof(LearningAdministrationStatisticsSource).Assembly.GetName().Name);
        Assert.Equal("HelpDev.Infrastructure", typeof(SearchAdministrationStatisticsSource).Assembly.GetName().Name);
        Assert.Equal("HelpDev.Infrastructure", typeof(OutboxAdministrationStatisticsSource).Assembly.GetName().Name);
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
