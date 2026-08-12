using HelpDev.API.Deployment;
using HelpDev.SharedContracts.Observability;
using Microsoft.Extensions.Options;

namespace HelpDev.API.Tests.Deployment;

[Trait("Category", "Deployment")]
public sealed class ApplicationReadinessStateTests
{
    [Fact]
    public void Starts_in_starting_state_and_is_not_accepting_traffic()
    {
        var state = new ApplicationReadinessState();

        Assert.Equal(ApplicationReadinessStatus.Starting, state.Status);
        Assert.False(state.IsAcceptingTraffic);
    }

    [Fact]
    public void Mark_ready_accepts_traffic()
    {
        var state = new ApplicationReadinessState();
        state.MarkReady();

        Assert.Equal(ApplicationReadinessStatus.Ready, state.Status);
        Assert.True(state.IsAcceptingTraffic);
    }

    [Fact]
    public void Mark_stopping_stops_accepting_traffic()
    {
        var state = new ApplicationReadinessState();
        state.MarkReady();
        state.MarkStopping();

        Assert.Equal(ApplicationReadinessStatus.Stopping, state.Status);
        Assert.False(state.IsAcceptingTraffic);
    }

    [Fact]
    public void Mark_failed_stops_accepting_traffic()
    {
        var state = new ApplicationReadinessState();
        state.MarkFailed();

        Assert.Equal(ApplicationReadinessStatus.Failed, state.Status);
        Assert.False(state.IsAcceptingTraffic);
    }
}

[Trait("Category", "Deployment")]
public sealed class ReleaseInfoProviderTests
{
    private sealed class FakeApplicationInfo : IApplicationInfo
    {
        public string ApplicationName => "HelpDev.API";
        public string Version => "9.9.9";
        public string? InformationalVersion => "9.9.9+abc";
        public string EnvironmentName => "Production";
    }

    private sealed class FakeLifetime : IApplicationLifetimeInfo
    {
        public DateTime StartedAtUtc { get; } = DateTime.UtcNow.AddMinutes(-5);
        public TimeSpan GetUptime() => TimeSpan.FromMinutes(5);
    }

    [Fact]
    public void Falls_back_to_assembly_version_when_release_version_absent()
    {
        var provider = new ReleaseInfoProvider(
            Options.Create(new ReleaseMetadataOptions()),
            new FakeApplicationInfo(),
            new FakeLifetime());

        var info = provider.GetReleaseInfo();

        Assert.Equal("9.9.9", info.Version);
        Assert.Equal("Production", info.Environment);
        Assert.True(info.UptimeSeconds >= 0);
    }

    [Fact]
    public void Uses_explicit_release_metadata_when_provided()
    {
        var provider = new ReleaseInfoProvider(
            Options.Create(new ReleaseMetadataOptions
            {
                Version = "1.0.0",
                Commit = "abcdef1",
                BuildTimestamp = "2026-07-21T08:00:00Z",
                Channel = "stable",
            }),
            new FakeApplicationInfo(),
            new FakeLifetime());

        var info = provider.GetReleaseInfo();

        Assert.Equal("1.0.0", info.Version);
        Assert.Equal("abcdef1", info.Commit);
        Assert.Equal("stable", info.Channel);
        Assert.Equal("2026-07-21T08:00:00Z", info.BuildTimestampUtc);
    }
}
