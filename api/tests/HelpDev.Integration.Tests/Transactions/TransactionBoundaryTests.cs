using HelpDev.Infrastructure.Persistence;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.Modules.Identity.Domain.Entities;
using HelpDev.Modules.Identity.Domain.Enums;
using HelpDev.Testing.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Integration.Tests.Transactions;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "PostgreSQL")]
public sealed class TransactionBoundaryTests : IntegrationTestClassBase
{
    public TransactionBoundaryTests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Validation_failure_rolls_back_entire_transaction()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var initialUserCount = await context.Users.CountAsync();

        await using var transaction = await context.Database.BeginTransactionAsync();

        var firstUserId = Guid.NewGuid();
        var sharedMobile = TestIds.Truncate($"09{Guid.NewGuid():N}", 11);
        context.Users.Add(new User
        {
            Id = firstUserId,
            Mobile = sharedMobile,
            FullName = "Transaction User",
            FirstName = "Transaction",
            LastName = "User",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Mobile = sharedMobile,
            FullName = "Duplicate Mobile",
            FirstName = "Duplicate",
            LastName = "Mobile",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        await transaction.RollbackAsync();

        await using var verifyScope = Factory.Services.CreateAsyncScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(initialUserCount, await verifyContext.Users.CountAsync());
    }

    [PostgreSqlFact]
    public async Task Successful_transaction_commits_all_changes()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var initialUserCount = await context.Users.CountAsync();

        await using var transaction = await context.Database.BeginTransactionAsync();

        var userId = Guid.NewGuid();
        var mobile = TestIds.Truncate($"09{Guid.NewGuid():N}", 11);
        context.Users.Add(new User
        {
            Id = userId,
            Mobile = mobile,
            FullName = "Committed User",
            FirstName = "Committed",
            LastName = "User",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        await using var verifyScope = Factory.Services.CreateAsyncScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await verifyContext.Users.SingleOrDefaultAsync(user => user.Id == userId);
        Assert.NotNull(user);
        Assert.Equal(mobile, user!.Mobile);
        Assert.Equal(initialUserCount + 1, await verifyContext.Users.CountAsync());
    }
}
