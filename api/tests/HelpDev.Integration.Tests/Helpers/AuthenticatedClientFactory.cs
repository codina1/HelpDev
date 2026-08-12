using System.Net.Http.Headers;
using HelpDev.Infrastructure.Persistence;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Identity.Domain.Entities;
using HelpDev.Modules.Identity.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Integration.Tests.Helpers;

public sealed class AuthenticatedClientFactory
{
    private readonly HelpDevWebApplicationFactory _factory;

    public AuthenticatedClientFactory(HelpDevWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public HttpClient CreateAnonymousClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

    public async Task<HttpClient> CreateUserClientAsync(CancellationToken cancellationToken = default)
    {
        var (client, _) = await CreateClientWithIdAsync(UserRole.User, cancellationToken);
        return client;
    }

    public async Task<HttpClient> CreateWriterClientAsync(CancellationToken cancellationToken = default)
    {
        var (client, _) = await CreateClientWithIdAsync(UserRole.Writer, cancellationToken);
        return client;
    }

    public async Task<HttpClient> CreateAdminClientAsync(CancellationToken cancellationToken = default)
    {
        var (client, _) = await CreateClientWithIdAsync(UserRole.Admin, cancellationToken);
        return client;
    }

    public Task<(HttpClient Client, Guid UserId)> CreateUserClientWithIdAsync(
        CancellationToken cancellationToken = default) =>
        CreateClientWithIdAsync(UserRole.User, cancellationToken);

    public Task<(HttpClient Client, Guid UserId)> CreateWriterClientWithIdAsync(
        CancellationToken cancellationToken = default) =>
        CreateClientWithIdAsync(UserRole.Writer, cancellationToken);

    public Task<(HttpClient Client, Guid UserId)> CreateAdminClientWithIdAsync(
        CancellationToken cancellationToken = default) =>
        CreateClientWithIdAsync(UserRole.Admin, cancellationToken);

    private async Task<(HttpClient Client, Guid UserId)> CreateClientWithIdAsync(
        UserRole role,
        CancellationToken cancellationToken)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

        var userId = Guid.NewGuid();
        var mobile = TestIds.Truncate($"09{Guid.NewGuid():N}", 11);

        context.Users.Add(new User
        {
            Id = userId,
            Mobile = mobile,
            FullName = $"{role} Test User",
            FirstName = role.ToString(),
            LastName = "User",
            Role = role,
            CreatedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync(cancellationToken);

        var (token, _) = jwt.GenerateToken(userId, role, mobile);
        var client = CreateAnonymousClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (client, userId);
    }
}
