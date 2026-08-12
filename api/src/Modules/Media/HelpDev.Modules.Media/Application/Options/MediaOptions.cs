using Microsoft.Extensions.Options;

namespace HelpDev.Modules.Media.Application.Options;

/// <summary>
/// Strongly typed Media options. Editorial/security limits — never accepted from request payload.
/// </summary>
public sealed class MediaOptions
{
    public const string SectionName = "Media";

    /// <summary>Default 5 MiB.</summary>
    public long MaxUploadBytes { get; set; } = 5 * 1024 * 1024;

    public int MaxWidth { get; set; } = 8192;

    public int MaxHeight { get; set; } = 8192;

    public string[] AllowedContentTypes { get; set; } =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
    ];

    /// <summary>
    /// Absolute path to local storage root. Must be outside source-controlled directories.
    /// Empty → resolved at startup to LocalApplicationData/HelpDev/media-uploads.
    /// </summary>
    public string LocalStorageRoot { get; set; } = string.Empty;

    /// <summary>Public URL base path, e.g. /media.</summary>
    public string PublicBasePath { get; set; } = "/media";

    public int MaxOriginalFileNameLength { get; set; } = 200;

    public int MaxAltTextLength { get; set; } = 200;

    public int MaxCaptionLength { get; set; } = 500;
}

public sealed class MediaOptionsValidator : IValidateOptions<MediaOptions>
{
    public ValidateOptionsResult Validate(string? name, MediaOptions options)
    {
        var failures = new List<string>();

        if (options.MaxUploadBytes is < 1024 or > 50 * 1024 * 1024)
        {
            failures.Add("Media:MaxUploadBytes must be between 1 KiB and 50 MiB.");
        }

        if (options.MaxWidth is < 1 or > 20000 || options.MaxHeight is < 1 or > 20000)
        {
            failures.Add("Media:MaxWidth/MaxHeight must be between 1 and 20000.");
        }

        if (options.AllowedContentTypes is null || options.AllowedContentTypes.Length == 0)
        {
            failures.Add("Media:AllowedContentTypes must not be empty.");
        }
        else
        {
            foreach (var type in options.AllowedContentTypes)
            {
                if (!Domain.ValueObjects.MediaContentType.Allowed.Contains(type))
                {
                    failures.Add($"Media:AllowedContentTypes contains unsupported type '{type}'.");
                }
            }
        }

        if (string.IsNullOrWhiteSpace(options.PublicBasePath)
            || !options.PublicBasePath.StartsWith("/", StringComparison.Ordinal)
            || options.PublicBasePath.Contains("..", StringComparison.Ordinal))
        {
            failures.Add("Media:PublicBasePath must be a root-relative path without '..'.");
        }

        if (options.MaxOriginalFileNameLength is < 16 or > 260)
        {
            failures.Add("Media:MaxOriginalFileNameLength must be between 16 and 260.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
