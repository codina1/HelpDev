using HelpDev.SharedContracts.Observability;

using HelpDev.SharedKernel.Time;



namespace HelpDev.Infrastructure.Tests.Observability;



internal sealed class FakeDateTimeProvider : IDateTimeProvider

{

    public FakeDateTimeProvider(DateTime utcNow) =>

        UtcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);



    public DateTime UtcNow { get; private set; }



    public void Advance(TimeSpan duration) => UtcNow = UtcNow.Add(duration);



    public void SetUtcNow(DateTime utcNow) =>

        UtcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);

}



internal sealed class FakePostgreSqlHealthProbe : IPostgreSqlHealthProbe

{

    public PostgreSqlHealthProbeResult Result { get; set; } =

        new(true, 10, OperationalHealthStates.Healthy, null);



    public Task<PostgreSqlHealthProbeResult> CheckAsync(CancellationToken cancellationToken = default) =>

        Task.FromResult(Result);

}



internal sealed class FakeOutboxOperationalQueries : IOutboxOperationalQueries

{

    public OutboxOperationalSnapshot Snapshot { get; set; } = HealthySnapshot(DateTime.UtcNow);



    public Task<OutboxOperationalSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default) =>

        Task.FromResult(Snapshot);



    public static OutboxOperationalSnapshot HealthySnapshot(DateTime checkedAtUtc) =>

        new(0, 0, 0, 0, null, checkedAtUtc, null, ProcessorEnabled: true, checkedAtUtc);

}



internal sealed class FakeSearchOperationalQueries : ISearchOperationalQueries

{

    public SearchOperationalSnapshot Snapshot { get; set; } =

        new(0, 0, null, DateTime.UtcNow, DateTime.UtcNow, true, DateTime.UtcNow);



    public Task<SearchOperationalSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default) =>

        Task.FromResult(Snapshot);

}



internal sealed class FakeAnalyticsOperationalQueries : IAnalyticsOperationalQueries

{

    public AnalyticsOperationalSnapshot Snapshot { get; set; } =

        new(0, 0, DateTime.UtcNow, null, true, DateTime.UtcNow);



    public Task<AnalyticsOperationalSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default) =>

        Task.FromResult(Snapshot);

}



internal sealed class FakeAuditOperationalQueries : IAuditOperationalQueries

{

    public AuditOperationalSnapshot Snapshot { get; set; } =

        new(true, DateTime.UtcNow, 0, DateTime.UtcNow);



    public Task<AuditOperationalSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default) =>

        Task.FromResult(Snapshot);

}



internal sealed class FakeApplicationInfo : IApplicationInfo

{

    public string ApplicationName { get; init; } = "HelpDev.API";



    public string Version { get; init; } = "1.0.0";



    public string? InformationalVersion { get; init; } = "1.0.0+test";



    public string EnvironmentName { get; init; } = "Test";

}



internal sealed class FakeApplicationLifetimeInfo : IApplicationLifetimeInfo

{

    public FakeApplicationLifetimeInfo(DateTime startedAtUtc) => StartedAtUtc = startedAtUtc;



    public DateTime StartedAtUtc { get; }



    public TimeSpan GetUptime() => DateTime.UtcNow - StartedAtUtc;

}


