using HelpDev.Infrastructure.Observability;

using HelpDev.SharedContracts.Observability;



namespace HelpDev.Observability.Tests;



public sealed class HealthSnapshotCacheTests

{

    [Fact]

    public void TryGet_returns_false_when_entry_missing()

    {

        var cache = new HealthSnapshotCache();



        var found = cache.TryGet("postgresql", out var result);



        Assert.False(found);

        Assert.Null(result);

    }



    [Fact]

    public void TryGet_returns_cached_entry_before_expiry()

    {

        var cache = new HealthSnapshotCache();

        var cached = new CachedHealthResult(

            OperationalHealthStates.Healthy,

            DateTime.UtcNow.AddSeconds(30),

            IsFailure: false);

        cache.Set("postgresql", cached);



        var found = cache.TryGet("postgresql", out var result);



        Assert.True(found);

        Assert.Equal(cached, result);

    }



    [Fact]

    public void TryGet_returns_false_when_entry_expired()

    {

        var cache = new HealthSnapshotCache();

        cache.Set("postgresql", new CachedHealthResult(

            OperationalHealthStates.Degraded,

            DateTime.UtcNow.AddSeconds(-1),

            IsFailure: true));



        var found = cache.TryGet("postgresql", out var result);



        Assert.False(found);

        Assert.Null(result);

    }



    [Fact]

    public void Set_overwrites_existing_entry()

    {

        var cache = new HealthSnapshotCache();

        cache.Set("outbox", new CachedHealthResult(

            OperationalHealthStates.Degraded,

            DateTime.UtcNow.AddSeconds(30),

            IsFailure: true));

        var updated = new CachedHealthResult(

            OperationalHealthStates.Healthy,

            DateTime.UtcNow.AddSeconds(30),

            IsFailure: false);

        cache.Set("outbox", updated);



        Assert.True(cache.TryGet("outbox", out var result));

        Assert.Equal(OperationalHealthStates.Healthy, result!.Status);

    }

}


