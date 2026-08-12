using Microsoft.Extensions.Options;

namespace HelpDev.SharedInfrastructure.Outbox;

public sealed class OutboxOptionsValidator : IValidateOptions<OutboxOptions>
{
    public ValidateOptionsResult Validate(string? name, OutboxOptions options)
    {
        if (options.BatchSize <= 0)
        {
            return ValidateOptionsResult.Fail("Outbox BatchSize must be greater than zero.");
        }

        if (options.PollIntervalSeconds <= 0)
        {
            return ValidateOptionsResult.Fail("Outbox PollIntervalSeconds must be greater than zero.");
        }

        if (options.LockDurationSeconds <= 0)
        {
            return ValidateOptionsResult.Fail("Outbox LockDurationSeconds must be greater than zero.");
        }

        if (options.MaxAttempts <= 0)
        {
            return ValidateOptionsResult.Fail("Outbox MaxAttempts must be greater than zero.");
        }

        return ValidateOptionsResult.Success;
    }
}
