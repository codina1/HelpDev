namespace HelpDev.SharedKernel.Ids;

/// <summary>
/// Base type for strongly typed identifiers wrapping a primitive value.
/// </summary>
public abstract class StronglyTypedId<TValue> : IEquatable<StronglyTypedId<TValue>>
    where TValue : notnull
{
    protected StronglyTypedId(TValue value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public TValue Value { get; }

    public override bool Equals(object? obj) =>
        obj is StronglyTypedId<TValue> other && Equals(other);

    public bool Equals(StronglyTypedId<TValue>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return EqualityComparer<TValue>.Default.Equals(Value, other.Value);
    }

    public override int GetHashCode() =>
        HashCode.Combine(GetType(), Value);

    public override string ToString() => Value.ToString() ?? string.Empty;

    public static bool operator ==(StronglyTypedId<TValue>? left, StronglyTypedId<TValue>? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(StronglyTypedId<TValue>? left, StronglyTypedId<TValue>? right) =>
        !(left == right);
}
