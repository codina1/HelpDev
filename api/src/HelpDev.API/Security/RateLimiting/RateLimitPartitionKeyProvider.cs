using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HelpDev.Modules.Identity.Application.Auth;
using Microsoft.AspNetCore.HttpOverrides;

namespace HelpDev.API.Security.RateLimiting;

public interface IRateLimitPartitionKeyProvider
{
    string GetAuthenticatedPartition(Guid userId);

    string GetAnonymousNetworkPartition(HttpContext httpContext);

    string GetOtpTargetPartition(string normalizedPhone);

    Guid? TryGetAuthenticatedUserId(HttpContext httpContext);
}

public sealed class RateLimitPartitionKeyProvider : IRateLimitPartitionKeyProvider
{
    private readonly byte[] _hashKey;

    public RateLimitPartitionKeyProvider(SecurityOptions securityOptions)
    {
        if (string.IsNullOrWhiteSpace(securityOptions.PartitionHashKey) || securityOptions.PartitionHashKey.Length < 32)
        {
            throw new InvalidOperationException("Security partition hash key must be at least 32 characters.");
        }

        _hashKey = Encoding.UTF8.GetBytes(securityOptions.PartitionHashKey);
    }

    public string GetAuthenticatedPartition(Guid userId) => $"user:{userId}";

    public string GetAnonymousNetworkPartition(HttpContext httpContext)
    {
        var remoteIp = httpContext.Connection.RemoteIpAddress;
        if (remoteIp is null)
        {
            return "anon:unknown";
        }

        var normalized = NormalizeIp(remoteIp);
        return $"anon:{ComputeHmac(normalized)}";
    }

    public string GetOtpTargetPartition(string normalizedPhone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedPhone);
        return $"otp-target:{ComputeHmac(normalizedPhone)}";
    }

    public Guid? TryGetAuthenticatedUserId(HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirstValue(JwtClaimTypes.UserId)
            ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private string ComputeHmac(string value)
    {
        using var hmac = new HMACSHA256(_hashKey);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant()[..16];
    }

    private static string NormalizeIp(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        return address.ToString().ToLowerInvariant();
    }
}

public static class ForwardedHeadersConfiguration
{
    public static void Configure(WebApplicationBuilder builder, SecurityOptions securityOptions)
    {
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = 2;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();

            foreach (var network in securityOptions.TrustedProxyNetworks)
            {
                if (TryParseCidr(network, out var parsedNetwork))
                {
                    options.KnownNetworks.Add(parsedNetwork);
                }
            }

            foreach (var proxy in securityOptions.TrustedProxyAddresses)
            {
                if (IPAddress.TryParse(proxy, out var address))
                {
                    options.KnownProxies.Add(address);
                }
            }
        });
    }

    private static bool TryParseCidr(string cidr, out Microsoft.AspNetCore.HttpOverrides.IPNetwork network)
    {
        network = null!;
        var parts = cidr.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out var prefix) || !int.TryParse(parts[1], out var prefixLength))
        {
            return false;
        }

        network = new Microsoft.AspNetCore.HttpOverrides.IPNetwork(prefix, prefixLength);
        return true;
    }
}
