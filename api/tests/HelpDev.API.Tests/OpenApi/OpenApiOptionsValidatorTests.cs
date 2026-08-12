using HelpDev.API.OpenApi;
using Microsoft.Extensions.Options;

namespace HelpDev.API.Tests.OpenApi;

public sealed class OpenApiOptionsValidatorTests
{
    private readonly OpenApiOptionsValidator _validator = new();

    [Fact]
    public void Disabled_openapi_skips_document_requirements()
    {
        var result = _validator.Validate(
            OpenApiOptions.SectionName,
            new OpenApiOptions
            {
                Enabled = false,
                ExposePublicDocument = false,
                ExposeAuthenticatedDocument = false,
                ExposeAdminDocument = false,
                ExposeCompleteDocument = false,
            });

        Assert.Equal(ValidateOptionsResult.Success, result);
    }

    [Fact]
    public void Enabled_openapi_requires_at_least_one_document()
    {
        var result = _validator.Validate(
            OpenApiOptions.SectionName,
            new OpenApiOptions
            {
                Enabled = true,
                ExposePublicDocument = false,
                ExposeAuthenticatedDocument = false,
                ExposeAdminDocument = false,
                ExposeCompleteDocument = false,
            });

        Assert.NotEqual(ValidateOptionsResult.Success, result);
        Assert.Contains("At least one OpenAPI document must be enabled", result.FailureMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void Export_directory_rejects_path_traversal()
    {
        var result = _validator.Validate(
            OpenApiOptions.SectionName,
            new OpenApiOptions
            {
                ExportDirectory = "../artifacts/openapi",
            });

        Assert.NotEqual(ValidateOptionsResult.Success, result);
        Assert.Contains("relative path without traversal", result.FailureMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void Export_directory_rejects_rooted_paths()
    {
        var rooted = Path.IsPathRooted("/tmp/openapi")
            ? "/tmp/openapi"
            : @"C:\artifacts\openapi";

        var result = _validator.Validate(
            OpenApiOptions.SectionName,
            new OpenApiOptions
            {
                ExportDirectory = rooted,
            });

        Assert.NotEqual(ValidateOptionsResult.Success, result);
        Assert.Contains("relative path without traversal", result.FailureMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void Valid_configuration_succeeds()
    {
        var result = _validator.Validate(
            OpenApiOptions.SectionName,
            new OpenApiOptions
            {
                ExportDirectory = "artifacts/openapi",
            });

        Assert.Equal(ValidateOptionsResult.Success, result);
    }
}
