namespace HelpDev.SharedApplication.Abstractions.Security;

public interface ICurrentUser
{
    Guid? UserId { get; }

    string? Mobile { get; }

    IReadOnlyCollection<string> Roles { get; }

    bool IsAuthenticated { get; }
}
