using HelpDev.SharedKernel.Time;

namespace HelpDev.Identity.Tests.Fakes;

internal sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public FakeDateTimeProvider(DateTime utcNow)
    {
        UtcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
    }

    public DateTime UtcNow { get; private set; }

    public void Advance(TimeSpan duration) => UtcNow = UtcNow.Add(duration);

    public void SetUtcNow(DateTime utcNow) =>
        UtcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
}
