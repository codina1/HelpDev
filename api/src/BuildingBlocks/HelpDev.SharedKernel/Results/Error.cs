namespace HelpDev.SharedKernel.Results;

public sealed class Error : IEquatable<Error>
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public Error(string code, string message)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    public string Code { get; }

    public string Message { get; }

    public bool Equals(Error? other)
    {
        if (other is null)
        {
            return false;
        }

        return string.Equals(Code, other.Code, StringComparison.Ordinal)
            && string.Equals(Message, other.Message, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj) => obj is Error other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Code, Message);

    public static bool operator ==(Error? left, Error? right)
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

    public static bool operator !=(Error? left, Error? right) => !(left == right);

    public override string ToString() =>
        string.IsNullOrEmpty(Code) ? Message : $"{Code}: {Message}";
}
