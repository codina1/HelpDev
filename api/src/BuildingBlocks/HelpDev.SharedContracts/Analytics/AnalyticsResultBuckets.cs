namespace HelpDev.SharedContracts.Analytics;

public static class AnalyticsResultBuckets
{
    public const string Zero = "0";
    public const string OneToFive = "1-5";
    public const string SixToTwenty = "6-20";
    public const string TwentyOnePlus = "21+";

    public static string FromResultCount(int totalResults) =>
        totalResults switch
        {
            0 => Zero,
            <= 5 => OneToFive,
            <= 20 => SixToTwenty,
            _ => TwentyOnePlus,
        };
}
