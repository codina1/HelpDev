using HelpDev.SharedContracts.Observability;



namespace HelpDev.Observability.Tests;



public sealed class OperationalBucketFormatterTests

{

    [Theory]

    [InlineData(0, "0")]

    [InlineData(1, "1-100")]

    [InlineData(100, "1-100")]

    [InlineData(101, "101-1000")]

    [InlineData(1000, "101-1000")]

    [InlineData(1001, "1001+")]

    public void PendingBucket_maps_count_to_expected_bucket(long count, string expected)

    {

        Assert.Equal(expected, OperationalBucketFormatter.PendingBucket(count));

    }



    [Theory]

    [InlineData(null, "unknown")]

    [InlineData(0, "under_1m")]

    [InlineData(59, "under_1m")]

    [InlineData(60, "1-5m")]

    [InlineData(299, "1-5m")]

    [InlineData(300, "5-30m")]

    [InlineData(1799, "5-30m")]

    [InlineData(1800, "30m+")]

    public void AgeBucket_maps_age_to_expected_bucket(int? totalSeconds, string expected)

    {

        TimeSpan? age = totalSeconds.HasValue ? TimeSpan.FromSeconds(totalSeconds.Value) : null;



        Assert.Equal(expected, OperationalBucketFormatter.AgeBucket(age));

    }



    [Theory]

    [InlineData(0, "under_50ms")]

    [InlineData(49, "under_50ms")]

    [InlineData(50, "under_500ms")]

    [InlineData(499, "under_500ms")]

    [InlineData(500, "under_2s")]

    [InlineData(1999, "under_2s")]

    [InlineData(2000, "over_2s")]

    public void LatencyBucket_maps_latency_to_expected_bucket(long milliseconds, string expected)

    {

        Assert.Equal(expected, OperationalBucketFormatter.LatencyBucket(milliseconds));

    }

}


