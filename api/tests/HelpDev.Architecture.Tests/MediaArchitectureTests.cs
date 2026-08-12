using HelpDev.Modules.Media.Application.Assets;
using HelpDev.Modules.Media.Application.Options;
using HelpDev.Modules.Media.Application.Storage;
using HelpDev.Modules.Media.Infrastructure.Storage;
using MediaModuleMarker = HelpDev.Modules.Media.ModuleMarker;
using ContentModuleMarker = HelpDev.Modules.Content.ModuleMarker;
using NetArchTest.Rules;

namespace HelpDev.Architecture.Tests;

public sealed class MediaArchitectureTests
{
    [Fact]
    public void Media_Domain_has_no_EF_or_AspNetCore()
    {
        var result = Types.InAssembly(typeof(MediaModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Domain")
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Media_Application_has_no_Infrastructure_reference()
    {
        var result = Types.InAssembly(typeof(MediaModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Application")
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Modules.Media.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Content_module_does_not_reference_Media_module()
    {
        var result = Types.InAssembly(typeof(ContentModuleMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Modules.Media")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void MediaAssetDto_has_no_domain_types()
    {
        Assert.DoesNotContain(
            typeof(MediaAssetDto).GetProperties(),
            p => p.PropertyType.Namespace?.StartsWith("HelpDev.Modules.Media.Domain", StringComparison.Ordinal) == true);
    }

    [Fact]
    public void Image_inspector_has_no_HttpClient()
    {
        var result = Types.InAssembly(typeof(MediaModuleMarker).Assembly)
            .That()
            .ResideInNamespaceContaining(".Infrastructure.Inspection")
            .ShouldNot()
            .HaveDependencyOn("System.Net.Http")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void LocalMediaStorage_implements_application_storage_abstraction()
    {
        Assert.True(typeof(IMediaStorage).IsAssignableFrom(typeof(LocalMediaStorage)));
    }

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}
