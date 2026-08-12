using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Analytics.Domain.Metrics;

public sealed class DailyActiveUser
{
    private DailyActiveUser()
    {
    }

    public DateOnly DateUtc { get; private set; }

    public Guid UserId { get; private set; }

    public DateTime FirstSeenAtUtc { get; private set; }

    public static DailyActiveUser Create(DateOnly dateUtc, Guid userId, DateTime firstSeenAtUtc)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException(AnalyticsErrorCodes.EventProcessingFailed, "User id is required.");
        }

        return new DailyActiveUser
        {
            DateUtc = dateUtc,
            UserId = userId,
            FirstSeenAtUtc = firstSeenAtUtc,
        };
    }
}
