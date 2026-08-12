namespace HelpDev.SharedInfrastructure.Outbox;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public bool Enabled { get; set; } = true;

    public int BatchSize { get; set; } = 20;

    public int PollIntervalSeconds { get; set; } = 5;

    public int LockDurationSeconds { get; set; } = 30;

    public int MaxAttempts { get; set; } = 10;
}
