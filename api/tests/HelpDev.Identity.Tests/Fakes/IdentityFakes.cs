using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Identity.Application.Persistence;
using HelpDev.Modules.Identity.Domain.Entities;
using HelpDev.Modules.Identity.Domain.Enums;

namespace HelpDev.Identity.Tests.Fakes;

internal sealed class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<Guid, User> _byId = new();
    private readonly Dictionary<string, Guid> _byMobile = new(StringComparer.Ordinal);

    public int AddCount { get; private set; }

    public int UpdateCount { get; private set; }

    public IReadOnlyCollection<User> Users => _byId.Values.ToArray();

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _byId.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetByMobileAsync(string mobile, CancellationToken cancellationToken = default)
    {
        if (!_byMobile.TryGetValue(mobile, out var id))
        {
            return Task.FromResult<User?>(null);
        }

        return Task.FromResult<User?>(_byId[id]);
    }

    public Task<IReadOnlyList<User>> ListAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<User>>(_byId.Values.ToList());

    public Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _byId[user.Id] = user;
        _byMobile[user.Mobile] = user.Id;
        AddCount++;
        return Task.FromResult(user);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _byId[user.Id] = user;
        _byMobile[user.Mobile] = user.Id;
        UpdateCount++;
        return Task.CompletedTask;
    }

    public void Seed(User user)
    {
        _byId[user.Id] = user;
        _byMobile[user.Mobile] = user.Id;
    }
}

internal sealed class FakeOtpStore : IOtpStore
{
    private readonly Dictionary<string, (string Code, DateTime ExpiresAt)> _entries = new(StringComparer.Ordinal);

    public IReadOnlyDictionary<string, (string Code, DateTime ExpiresAt)> Entries => _entries;

    public Task StoreAsync(
        string mobile,
        string code,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        _entries[mobile] = (code, DateTime.UtcNow.Add(expiration));
        return Task.CompletedTask;
    }

    public Task<bool> ValidateAndRemoveAsync(
        string mobile,
        string code,
        CancellationToken cancellationToken = default)
    {
        if (!_entries.TryGetValue(mobile, out var entry))
        {
            return Task.FromResult(false);
        }

        if (entry.ExpiresAt <= DateTime.UtcNow)
        {
            _entries.Remove(mobile);
            return Task.FromResult(false);
        }

        if (!string.Equals(entry.Code, code, StringComparison.Ordinal))
        {
            return Task.FromResult(false);
        }

        _entries.Remove(mobile);
        return Task.FromResult(true);
    }

    public void Seed(string mobile, string code, TimeSpan lifetime) =>
        _entries[mobile] = (code, DateTime.UtcNow.Add(lifetime));
}

internal sealed class FakeJwtTokenService : IJwtTokenService
{
    public Guid? LastUserId { get; private set; }

    public UserRole? LastRole { get; private set; }

    public string? LastMobile { get; private set; }

    public int CallCount { get; private set; }

    public string TokenToReturn { get; set; } = "test-access-token";

    public int ExpiresInSecondsToReturn { get; set; } = 3600;

    public (string Token, int ExpiresInSeconds) GenerateToken(Guid userId, UserRole role, string mobile)
    {
        CallCount++;
        LastUserId = userId;
        LastRole = role;
        LastMobile = mobile;
        return (TokenToReturn, ExpiresInSecondsToReturn);
    }
}
