using HelpDev.SharedKernel.Time;

namespace HelpDev.SharedInfrastructure.Time;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
