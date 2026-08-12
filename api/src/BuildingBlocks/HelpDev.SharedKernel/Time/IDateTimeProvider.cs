namespace HelpDev.SharedKernel.Time;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
