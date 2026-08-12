using Microsoft.Extensions.Options;

namespace HelpDev.API.OpenApi;

public sealed class OpenApiOptions
{
    public const string SectionName = "OpenApi";

    public bool Enabled { get; set; } = true;

    public bool EnableUi { get; set; } = true;

    public bool EnableInProduction { get; set; }

    public bool ExposePublicDocument { get; set; } = true;

    public bool ExposeAuthenticatedDocument { get; set; } = true;

    public bool ExposeAdminDocument { get; set; } = true;

    public bool ExposeCompleteDocument { get; set; } = true;

    public bool ExposeAdminDocumentInProduction { get; set; }

    public bool EnableTryItOutInProduction { get; set; }

    public string ExportDirectory { get; set; } = "artifacts/openapi";
}

public sealed class OpenApiOptionsValidator : IValidateOptions<OpenApiOptions>
{
    public ValidateOptionsResult Validate(string? name, OpenApiOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (!options.ExposePublicDocument
            && !options.ExposeAuthenticatedDocument
            && !options.ExposeAdminDocument
            && !options.ExposeCompleteDocument)
        {
            return ValidateOptionsResult.Fail("At least one OpenAPI document must be enabled when OpenAPI is enabled.");
        }

        if (string.IsNullOrWhiteSpace(options.ExportDirectory))
        {
            return ValidateOptionsResult.Fail("OpenAPI export directory is required.");
        }

        if (Path.IsPathRooted(options.ExportDirectory)
            || options.ExportDirectory.Contains("..", StringComparison.Ordinal))
        {
            return ValidateOptionsResult.Fail("OpenAPI export directory must be a relative path without traversal.");
        }

        return ValidateOptionsResult.Success;
    }
}
