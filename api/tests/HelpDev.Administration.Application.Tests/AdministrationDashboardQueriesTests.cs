using HelpDev.Administration.Application.Tests.Fakes;
using HelpDev.Modules.Administration.Application;
using HelpDev.Modules.Administration.Application.Dashboard;

namespace HelpDev.Administration.Application.Tests;

public sealed class AdministrationDashboardQueriesTests
{
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 7, 19, 15, 0, 0, DateTimeKind.Utc));

    [Fact]
    public async Task GetAsync_composes_all_sources_and_limits_recent_items()
    {
        var identity = new FakeIdentitySource
        {
            Stats = new IdentityAdministrationStatistics(
                10,
                4,
                2,
                [
                    new RecentAdminActivityDto("user", Guid.NewGuid(), "U1", _clock.UtcNow.AddMinutes(-1)),
                    new RecentAdminActivityDto("user", Guid.NewGuid(), "U2", _clock.UtcNow.AddMinutes(-2)),
                ]),
        };
        var content = new FakeContentSource
        {
            Stats = new ContentAdministrationStatistics(
                20,
                12,
                8,
                null,
                [new RecentAdminActivityDto("content", Guid.NewGuid(), "C1", _clock.UtcNow.AddMinutes(-3))]),
        };
        var learning = new FakeLearningSource
        {
            Stats = new LearningAdministrationStatistics(
                5,
                3,
                40,
                1,
                [new RecentAdminActivityDto("course", Guid.NewGuid(), "L1", _clock.UtcNow)]),
        };
        var search = new FakeSearchSource
        {
            Stats = new SearchAdministrationStatistics(100, 80, _clock.UtcNow.AddHours(-1)),
        };
        var outbox = new FakeOutboxSource
        {
            Stats = new OutboxAdministrationStatistics(1, 0, 2, 9, _clock.UtcNow.AddDays(-1), _clock.UtcNow),
        };

        var analytics = new FakeAnalyticsSource
        {
            Stats = new AnalyticsAdministrationStatistics(0, 0, 0, 0, 0, 0, 0, 0m),
        };

        using var cts = new CancellationTokenSource();
        var sut = new AdministrationDashboardQueries(identity, content, learning, search, outbox, analytics, _clock);

        var dto = await sut.GetAsync(cts.Token);

        Assert.Equal(10, dto.Users.TotalUsers);
        Assert.Equal(4, dto.Users.ActiveUsers);
        Assert.Equal(12, dto.Content.PublishedContent);
        Assert.Null(dto.Content.PublicationsToday);
        Assert.Equal(3, dto.Learning.PublishedCourses);
        Assert.Equal(100, dto.Search.TotalSearchDocuments);
        Assert.Equal(2, dto.Outbox.Failed);
        Assert.Equal(4, dto.RecentItems.Count);
        Assert.Equal("course", dto.RecentItems[0].Category);
        Assert.True(identity.Called);
        Assert.True(content.Called);
        Assert.True(learning.Called);
        Assert.True(search.Called);
        Assert.True(outbox.Called);
        Assert.Equal(cts.Token, identity.LastToken);
    }

    [Fact]
    public async Task GetAsync_wraps_source_failures()
    {
        var sut = new AdministrationDashboardQueries(
            new FakeIdentitySource { Throw = true },
            new FakeContentSource(),
            new FakeLearningSource(),
            new FakeSearchSource(),
            new FakeOutboxSource(),
            new FakeAnalyticsSource(),
            _clock);

        var ex = await Assert.ThrowsAsync<AdministrationException>(() => sut.GetAsync());

        Assert.Equal(AdministrationApplicationErrorCodes.DashboardUnavailable, ex.Code);
    }

    private sealed class FakeIdentitySource : IIdentityAdministrationStatisticsSource
    {
        public IdentityAdministrationStatistics Stats { get; set; } =
            new(0, 0, 0, []);

        public bool Throw { get; set; }

        public bool Called { get; private set; }

        public CancellationToken LastToken { get; private set; }

        public Task<IdentityAdministrationStatistics> GetAsync(
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            Called = true;
            LastToken = cancellationToken;
            if (Throw)
            {
                throw new InvalidOperationException("identity failed");
            }

            return Task.FromResult(Stats);
        }
    }

    private sealed class FakeContentSource : IContentAdministrationStatisticsSource
    {
        public ContentAdministrationStatistics Stats { get; set; } =
            new(0, 0, 0, null, []);

        public bool Called { get; private set; }

        public Task<ContentAdministrationStatistics> GetAsync(
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            Called = true;
            return Task.FromResult(Stats);
        }
    }

    private sealed class FakeLearningSource : ILearningAdministrationStatisticsSource
    {
        public LearningAdministrationStatistics Stats { get; set; } =
            new(0, 0, 0, 0, []);

        public bool Called { get; private set; }

        public Task<LearningAdministrationStatistics> GetAsync(
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            Called = true;
            return Task.FromResult(Stats);
        }
    }

    private sealed class FakeSearchSource : ISearchAdministrationStatisticsSource
    {
        public SearchAdministrationStatistics Stats { get; set; } =
            new(0, 0, null);

        public bool Called { get; private set; }

        public Task<SearchAdministrationStatistics> GetAsync(CancellationToken cancellationToken = default)
        {
            Called = true;
            return Task.FromResult(Stats);
        }
    }

    private sealed class FakeOutboxSource : IOutboxAdministrationStatisticsSource
    {
        public OutboxAdministrationStatistics Stats { get; set; } =
            new(0, 0, 0, 0, null, null);

        public bool Called { get; private set; }

        public Task<OutboxAdministrationStatistics> GetAsync(CancellationToken cancellationToken = default)
        {
            Called = true;
            return Task.FromResult(Stats);
        }
    }

    private sealed class FakeAnalyticsSource : IAnalyticsAdministrationStatisticsSource
    {
        public AnalyticsAdministrationStatistics Stats { get; set; } =
            new(0, 0, 0, 0, 0, 0, 0, 0m);

        public bool Called { get; private set; }

        public Task<AnalyticsAdministrationStatistics> GetAsync(CancellationToken cancellationToken = default)
        {
            Called = true;
            return Task.FromResult(Stats);
        }
    }
}
