using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Learning.Domain.Enrollments;

public sealed class ProgressPercentage : ValueObject
{
    private ProgressPercentage(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static ProgressPercentage Zero { get; } = new(0);

    public static ProgressPercentage Full { get; } = new(100);

    public static ProgressPercentage From(int value)
    {
        if (value is < 0 or > 100)
        {
            throw new DomainException("Progress percentage must be between 0 and 100.");
        }

        return value switch
        {
            0 => Zero,
            100 => Full,
            _ => new ProgressPercentage(value),
        };
    }

    /// <summary>
    /// Reconstitutes progress from persistence without re-validation.
    /// </summary>
    public static ProgressPercentage FromPersisted(int value) =>
        value switch
        {
            0 => Zero,
            100 => Full,
            _ => new ProgressPercentage(value),
        };

    public override string ToString() => Value.ToString();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
