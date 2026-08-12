using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Ai;

public sealed class AiProviderOptions
{
    public const string SectionName = "Ai";

    /// <summary>Master switch. When false, generators refuse requests.</summary>
    public bool Enabled { get; set; }

    /// <summary>Fake | Http — Fake is for tests/dev only.</summary>
    public string ProviderName { get; set; } = "Fake";

    public string Model { get; set; } = "fake-v1";

    /// <summary>Optional HTTP endpoint for ProviderName=Http (generic JSON POST).</summary>
    public string? Endpoint { get; set; }

    /// <summary>Optional API key for Http provider. Never logged.</summary>
    public string? ApiKey { get; set; }

    public int DefaultMaxTokens { get; set; } = 1024;

    /// <summary>Comma-separated ContentAiTaskType names. Empty = all tasks.</summary>
    public string AllowedTasks { get; set; } =
        "ContentAnalysis,TitleSuggestion,MetaDescription,OutlineGeneration,FaqGeneration";
}

public sealed class AiProviderOptionsValidator : IValidateOptions<AiProviderOptions>
{
    public ValidateOptionsResult Validate(string? name, AiProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.DefaultMaxTokens is < 16 or > 8192)
        {
            return ValidateOptionsResult.Fail("Ai:DefaultMaxTokens must be between 16 and 8192.");
        }

        var provider = (options.ProviderName ?? string.Empty).Trim();
        if (provider.Length == 0)
        {
            return ValidateOptionsResult.Fail("Ai:ProviderName is required.");
        }

        if (!string.Equals(provider, "Fake", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(provider, "Http", StringComparison.OrdinalIgnoreCase))
        {
            return ValidateOptionsResult.Fail("Ai:ProviderName must be Fake or Http.");
        }

        if (options.Enabled
            && string.Equals(provider, "Http", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(options.Endpoint)
                || !Uri.TryCreate(options.Endpoint.Trim(), UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            {
                return ValidateOptionsResult.Fail("Ai:Endpoint must be an absolute http(s) URL when ProviderName is Http.");
            }
        }

        if (string.IsNullOrWhiteSpace(options.Model) || options.Model.Length > 100)
        {
            return ValidateOptionsResult.Fail("Ai:Model is required and must be <= 100 characters.");
        }

        // Never include ApiKey in validation failure messages.
        return ValidateOptionsResult.Success;
    }
}
