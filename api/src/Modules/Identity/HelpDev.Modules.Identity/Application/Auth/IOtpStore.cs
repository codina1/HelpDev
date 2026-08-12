namespace HelpDev.Modules.Identity.Application.Auth;

public interface IOtpStore
{
    Task StoreAsync(string mobile, string code, TimeSpan expiration, CancellationToken cancellationToken = default);

    Task<bool> ValidateAndRemoveAsync(string mobile, string code, CancellationToken cancellationToken = default);
}
