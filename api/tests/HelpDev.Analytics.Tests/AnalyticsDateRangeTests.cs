using HelpDev.Modules.Analytics.Application;
using HelpDev.Modules.Analytics.Application.Queries;
using HelpDev.Modules.Analytics.Domain;

namespace HelpDev.Analytics.Tests;

public sealed class AnalyticsDateRangeTests
{
    private static readonly AnalyticsOptions Options = new() { MaxQueryRangeDays = 366 };

    [Fact]
    public void Valid_range_passes_validate()
    {
        var range = new AnalyticsDateRange(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31));

        range.Validate(Options);
    }

    [Fact]
    public void Same_day_range_passes_validate()
    {
        var today = new DateOnly(2026, 7, 20);
        var range = new AnalyticsDateRange(today, today);

        range.Validate(Options);
    }

    [Fact]
    public void From_after_to_throws()
    {
        var range = new AnalyticsDateRange(
            new DateOnly(2026, 7, 20),
            new DateOnly(2026, 7, 19));

        var ex = Assert.Throws<AnalyticsException>(() => range.Validate(Options));
        Assert.Equal(AnalyticsApplicationErrorCodes.DateRangeInvalid, ex.Code);
    }

    [Fact]
    public void Range_exceeding_max_days_throws()
    {
        var restrictive = new AnalyticsOptions { MaxQueryRangeDays = 30 };
        var range = new AnalyticsDateRange(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 4, 1));

        var ex = Assert.Throws<AnalyticsException>(() => range.Validate(restrictive));
        Assert.Equal(AnalyticsApplicationErrorCodes.DateRangeTooLarge, ex.Code);
    }

    [Fact]
    public void Last30Days_factory_creates_correct_range()
    {
        var today = new DateOnly(2026, 7, 20);
        var range = AnalyticsDateRange.Last30Days(today);

        Assert.Equal(today.AddDays(-29), range.FromUtc);
        Assert.Equal(today, range.ToUtc);
    }
}
