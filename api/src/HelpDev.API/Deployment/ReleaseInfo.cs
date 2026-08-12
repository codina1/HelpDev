using System.Reflection;
using HelpDev.SharedContracts.Observability;
using Microsoft.Extensions.Options;

namespace HelpDev.API.Deployment;

/// <summary>
/// Admin-only release metadata. Never includes source paths, machine/user names,
/// process arguments, repository URLs, or secrets.
/// </summary>
public sealed record ReleaseInfoDto(
    string Version,
    string? InformationalVersion,
    string? Commit,
    string? BuildTimestampUtc,
    string Channel,
    string Environment,
    long UptimeSeconds);

public interface IReleaseInfoProvider
{
    ReleaseInfoDto GetReleaseInfo();
}

public sealed class ReleaseInfoProvider : IReleaseInfoProvider
{
    private readonly ReleaseMetadataOptions _options;
    private readonly IApplicationInfo _applicationInfo;
    private readonly IApplicationLifetimeInfo _lifetime;

    public ReleaseInfoProvider(
        IOptions<ReleaseMetadataOptions> options,
        IApplicationInfo applicationInfo,
        IApplicationLifetimeInfo lifetime)
    {
        _options = options.Value;
        _applicationInfo = applicationInfo;
        _lifetime = lifetime;
    }

    public ReleaseInfoDto GetReleaseInfo()
    {
        var version = FirstNonEmpty(_options.Version, _applicationInfo.Version);
        var informational = FirstNonEmpty(
            _applicationInfo.InformationalVersion,
            Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);
        var channel = FirstNonEmpty(_options.Channel, _applicationInfo.EnvironmentName) ?? "unknown";

        return new ReleaseInfoDto(
            Version: version ?? "0.0.0",
            InformationalVersion: informational,
            Commit: NullIfEmpty(_options.Commit),
            BuildTimestampUtc: NullIfEmpty(_options.BuildTimestamp),
            Channel: channel,
            Environment: _applicationInfo.EnvironmentName,
            UptimeSeconds: (long)_lifetime.GetUptime().TotalSeconds);
    }

    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
