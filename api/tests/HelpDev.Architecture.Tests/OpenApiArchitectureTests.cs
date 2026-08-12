using System.Reflection;
using HelpDev.API.Contracts;
using HelpDev.API.Controllers;
using HelpDev.API.Extensions;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Identity.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using NetArchTest.Rules;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HelpDev.Architecture.Tests;

public sealed class OpenApiArchitectureTests
{
    [Fact]
    public void Public_controllers_have_ApiAudience_and_ApiVersion_except_TestController()
    {
        var controllers = typeof(ContentController).Assembly
            .GetTypes()
            .Where(type => type.IsClass
                && !type.IsAbstract
                && type.Namespace == "HelpDev.API.Controllers"
                && type.Name.EndsWith("Controller", StringComparison.Ordinal)
                && type.Name != nameof(TestController))
            .ToList();

        Assert.NotEmpty(controllers);

        var missingAudience = controllers
            .Where(controller => !HasApiAudience(controller))
            .Select(controller => controller.Name)
            .ToList();

        var missingVersion = controllers
            .Where(controller => !HasApiVersion(controller))
            .Select(controller => controller.Name)
            .ToList();

        Assert.Empty(missingAudience);
        Assert.Empty(missingVersion);
    }

    [Fact]
    public void Admin_controllers_require_AdminOnly_policy()
    {
        var adminControllers = typeof(ContentController).Assembly
            .GetTypes()
            .Where(type => type.IsClass
                && !type.IsAbstract
                && type.Namespace == "HelpDev.API.Controllers"
                && type.Name.EndsWith("Controller", StringComparison.Ordinal)
                && type.Name.Contains("Admin", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(adminControllers);

        var missingPolicy = adminControllers
            .Where(controller => !HasAdminOnlyAuthorize(controller))
            .Select(controller => controller.Name)
            .ToList();

        Assert.Empty(missingPolicy);
    }

    [Fact]
    public void OpenApi_filters_do_not_reference_module_Infrastructure()
    {
        var result = Types.InAssembly(typeof(OperationMetadataOperationFilter).Assembly)
            .That()
            .ResideInNamespace("HelpDev.API.OpenApi")
            .And()
            .ImplementInterface(typeof(IOperationFilter))
            .Or()
            .ImplementInterface(typeof(IDocumentFilter))
            .ShouldNot()
            .HaveDependencyOnAny(
                "HelpDev.Modules.Identity.Infrastructure",
                "HelpDev.Modules.Content.Infrastructure",
                "HelpDev.Modules.Learning.Infrastructure",
                "HelpDev.Modules.Search.Infrastructure",
                "HelpDev.Modules.Administration.Infrastructure",
                "HelpDev.Modules.Toolbox.Infrastructure",
                "HelpDev.Modules.PromptLab.Infrastructure",
                "HelpDev.Modules.Analytics.Infrastructure",
                "HelpDev.Modules.Auditing.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void HelpDev_API_csproj_does_not_reference_deprecated_Microsoft_AspNetCore_Mvc_Versioning()
    {
        var csprojPath = FindFile("HelpDev.API.csproj");
        var text = File.ReadAllText(csprojPath);

        Assert.DoesNotContain("Microsoft.AspNetCore.Mvc.Versioning", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApiErrorResponse_exposes_Message_and_Code()
    {
        var message = typeof(ApiErrorResponse).GetProperty(nameof(ApiErrorResponse.Message));
        var code = typeof(ApiErrorResponse).GetProperty(nameof(ApiErrorResponse.Code));

        Assert.NotNull(message);
        Assert.NotNull(code);
        Assert.Equal(typeof(string), message!.PropertyType);
        Assert.Equal(typeof(string), code!.PropertyType);
    }

    [Fact]
    public void OpenApiOptions_EnableInProduction_defaults_to_false()
    {
        var options = new OpenApiOptions();

        Assert.False(options.EnableInProduction);
    }

    [Fact]
    public void Controllers_do_not_expose_DbContext_types_in_action_signatures()
    {
        var violations = typeof(ContentController).Assembly
            .GetTypes()
            .Where(type => type.IsClass
                && !type.IsAbstract
                && type.Namespace == "HelpDev.API.Controllers"
                && type.Name.EndsWith("Controller", StringComparison.Ordinal))
            .SelectMany(controller => controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .SelectMany(method => method.GetParameters()
                    .Where(parameter => IsDbContextType(parameter.ParameterType))
                    .Select(parameter => $"{controller.Name}.{method.Name}({parameter.ParameterType.Name})")))
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void AudienceDocumentFilter_marks_legacy_api_health_as_deprecated()
    {
        var path = FindFile("OpenApiFilters.cs", "HelpDev.API");
        var text = File.ReadAllText(path);

        Assert.Contains("AddLegacyHealth", text, StringComparison.Ordinal);
        Assert.Contains("\"/api/health\"", text, StringComparison.Ordinal);
        Assert.Contains("Deprecated = true", text, StringComparison.Ordinal);
    }

    private static bool HasApiVersion(Type controller) =>
        controller.GetCustomAttributes(inherit: true)
            .Any(attribute => attribute.GetType().FullName == "Asp.Versioning.ApiVersionAttribute");

    private static bool HasApiAudience(Type controller) =>
        controller.GetCustomAttribute<ApiAudienceAttribute>() is not null
        || controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Any(method => method.GetCustomAttribute<ApiAudienceAttribute>() is not null);

    private static bool HasAdminOnlyAuthorize(Type controller)
    {
        var authorize = controller.GetCustomAttribute<AuthorizeAttribute>();
        if (authorize?.Policy == AuthorizationPolicies.AdminOnly)
        {
            return true;
        }

        return controller.GetCustomAttributes<AuthorizeAttribute>(inherit: true)
            .Any(attribute => attribute.Policy == AuthorizationPolicies.AdminOnly);
    }

    private static bool IsDbContextType(Type type) =>
        typeof(DbContext).IsAssignableFrom(type);

    private static string FindFile(string fileName, string? directoryHint = null)
    {
        var start = new DirectoryInfo(AppContext.BaseDirectory);
        for (var current = start; current is not null; current = current.Parent)
        {
            IEnumerable<FileInfo> matches;
            try
            {
                matches = current.GetFiles(fileName, SearchOption.AllDirectories);
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            var filtered = matches
                .Where(file => directoryHint is null
                    || file.DirectoryName?.Contains(directoryHint, StringComparison.OrdinalIgnoreCase) == true)
                .Where(file => !file.FullName.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                    && !file.FullName.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                .ToList();

            if (filtered.Count > 0)
            {
                return filtered[0].FullName;
            }
        }

        throw new FileNotFoundException($"Could not locate {fileName}.");
    }

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}
