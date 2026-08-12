using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace HelpDev.API.Deployment;

/// <summary>
/// Reverse proxy / forwarded-header trust configuration. Bound from "ReverseProxy".
/// </summary>
public sealed class ReverseProxyOptions
{
    public const string SectionName = "ReverseProxy";

    public bool Enabled { get; set; }

    public string[] TrustedProxyAddresses { get; set; } = [];

    public string[] TrustedProxyNetworks { get; set; } = [];

    public int ForwardLimit { get; set; } = 1;

    public bool RequireForwardedProto { get; set; } = true;

    public bool RequireKnownProxyConfiguration { get; set; } = true;

    public bool HasTrustedProxies =>
        TrustedProxyAddresses.Length > 0 || TrustedProxyNetworks.Length > 0;
}

/// <summary>
/// HTTPS redirection and HSTS policy. Bound from "Https".
/// </summary>
public sealed class HttpsPolicyOptions
{
    public const string SectionName = "Https";

    public bool RedirectToHttps { get; set; } = true;

    public bool EnableHsts { get; set; } = true;

    public int HstsMaxAgeDays { get; set; } = 365;

    public bool HstsIncludeSubDomains { get; set; } = true;

    public bool HstsPreload { get; set; }
}

/// <summary>
/// Release/version metadata reported to Admin operators. Bound from "Release".
/// </summary>
public sealed class ReleaseMetadataOptions
{
    public const string SectionName = "Release";

    public string? Version { get; set; }

    public string? Commit { get; set; }

    public string? BuildTimestamp { get; set; }

    public string? Channel { get; set; }
}

/// <summary>
/// Graceful shutdown configuration. Bound from "Shutdown".
/// </summary>
public sealed class ShutdownOptions
{
    public const string SectionName = "Shutdown";

    public int TimeoutSeconds { get; set; } = 30;
}

public sealed class ReverseProxyOptionsValidator : IValidateOptions<ReverseProxyOptions>
{
    private readonly IHostEnvironment _environment;

    public ReverseProxyOptionsValidator(IHostEnvironment environment)
    {
        _environment = environment;
    }

    public ValidateOptionsResult Validate(string? name, ReverseProxyOptions options)
    {
        if (options.ForwardLimit is < 1 or > 8)
        {
            return ValidateOptionsResult.Fail("Reverse proxy ForwardLimit must be between 1 and 8.");
        }

        foreach (var network in options.TrustedProxyNetworks)
        {
            if (!network.Contains('/', StringComparison.Ordinal))
            {
                return ValidateOptionsResult.Fail($"Trusted proxy network '{network}' is malformed (expected CIDR).");
            }
        }

        if (_environment.IsProduction() && options.Enabled)
        {
            if (options.RequireKnownProxyConfiguration && !options.HasTrustedProxies)
            {
                return ValidateOptionsResult.Fail(
                    "Reverse proxy is enabled but no trusted proxy addresses or networks are configured.");
            }
        }

        return ValidateOptionsResult.Success;
    }
}

public sealed class HttpsPolicyOptionsValidator : IValidateOptions<HttpsPolicyOptions>
{
    public ValidateOptionsResult Validate(string? name, HttpsPolicyOptions options)
    {
        if (options.HstsMaxAgeDays is < 0 or > 730)
        {
            return ValidateOptionsResult.Fail("HSTS max-age days must be between 0 and 730.");
        }

        if (options.EnableHsts && options.HstsMaxAgeDays == 0)
        {
            return ValidateOptionsResult.Fail("HSTS is enabled but max-age is zero.");
        }

        return ValidateOptionsResult.Success;
    }
}

public sealed class ShutdownOptionsValidator : IValidateOptions<ShutdownOptions>
{
    public ValidateOptionsResult Validate(string? name, ShutdownOptions options)
    {
        if (options.TimeoutSeconds is < 1 or > 300)
        {
            return ValidateOptionsResult.Fail("Shutdown timeout must be between 1 and 300 seconds.");
        }

        return ValidateOptionsResult.Success;
    }
}

public sealed class ReleaseMetadataOptionsValidator : IValidateOptions<ReleaseMetadataOptions>
{
    private static readonly Regex SafeToken = new(@"^[A-Za-z0-9._+\-/ ]+$", RegexOptions.Compiled);

    public ValidateOptionsResult Validate(string? name, ReleaseMetadataOptions options)
    {
        if (!ValidateToken(options.Version, 64, out var versionError))
        {
            return ValidateOptionsResult.Fail(versionError);
        }

        if (!ValidateToken(options.Commit, 64, out var commitError))
        {
            return ValidateOptionsResult.Fail(commitError);
        }

        if (!ValidateToken(options.Channel, 32, out var channelError))
        {
            return ValidateOptionsResult.Fail(channelError);
        }

        if (!string.IsNullOrWhiteSpace(options.BuildTimestamp)
            && !DateTimeOffset.TryParse(
                options.BuildTimestamp,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                out _))
        {
            return ValidateOptionsResult.Fail("Release build timestamp must be a valid UTC ISO 8601 value.");
        }

        return ValidateOptionsResult.Success;
    }

    private static bool ValidateToken(string? value, int maxLength, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (value.Length > maxLength)
        {
            error = $"Release metadata value exceeds the maximum length of {maxLength}.";
            return false;
        }

        if (value.Contains('\n', StringComparison.Ordinal) || value.Contains('\r', StringComparison.Ordinal))
        {
            error = "Release metadata values must not contain line breaks.";
            return false;
        }

        if (!SafeToken.IsMatch(value))
        {
            error = "Release metadata values contain unsafe characters.";
            return false;
        }

        return true;
    }
}
