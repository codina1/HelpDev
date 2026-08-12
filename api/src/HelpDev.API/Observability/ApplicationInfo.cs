using System.Reflection;
using HelpDev.SharedContracts.Observability;
using HelpDev.SharedKernel.Time;

namespace HelpDev.API.Observability;

public sealed class ApplicationInfo : IApplicationInfo
{
    public ApplicationInfo(IHostEnvironment environment)
    {
        EnvironmentName = environment.EnvironmentName;
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        ApplicationName = assembly.GetName().Name ?? "HelpDev.API";
        Version = assembly.GetName().Version?.ToString() ?? "0.0.0";
        InformationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    }

    public string ApplicationName { get; }

    public string Version { get; }

    public string? InformationalVersion { get; }

    public string EnvironmentName { get; }
}

public sealed class ApplicationLifetimeInfo : IApplicationLifetimeInfo
{
    private readonly IDateTimeProvider _clock;

    public ApplicationLifetimeInfo(IDateTimeProvider clock)
    {
        _clock = clock;
        StartedAtUtc = clock.UtcNow;
    }

    public DateTime StartedAtUtc { get; }

    public TimeSpan GetUptime() => _clock.UtcNow - StartedAtUtc;
}
