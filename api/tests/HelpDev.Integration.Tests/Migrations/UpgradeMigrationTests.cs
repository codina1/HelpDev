using HelpDev.Infrastructure.Persistence;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Identity.Domain.Entities;
using HelpDev.Modules.Identity.Domain.Enums;
using HelpDev.Testing.PostgreSQL;
using HelpDev.Testing.PostgreSQL.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HelpDev.Integration.Tests.Migrations;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "PostgreSQL")]
public sealed class UpgradeMigrationTests : PostgreSqlIntegrationTestBase
{
    private const string ToolboxMigrationId = "20260719155032_AddToolboxModuleV1";

    public UpgradeMigrationTests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Upgrade_from_toolbox_v1_preserves_seeded_identity_and_content_data()
    {
        var connectionString = await Fixture.CreateIsolatedDatabaseAsync();
        await using var context = await CreateContextAtMigrationAsync(connectionString, ToolboxMigrationId);

        var userId = Guid.NewGuid();
        var contentId = Guid.NewGuid();
        const string mobile = "09120001122";
        const string slug = "upgrade-test-slug";

        context.Users.Add(new User
        {
            Id = userId,
            Mobile = mobile,
            FullName = "Upgrade Test",
            FirstName = "Upgrade",
            LastName = "Test",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        // Seed content at the OLD (pre-CMS-fields) schema via raw SQL so the upgrade path and
        // AddContentCmsFieldsV1 backfill (updated_at/published_at_utc) are exercised faithfully.
        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO contents ("Id", title, slug, body, type, author_id, status, views, saves, created_at)
            VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})
            """,
            contentId,
            "Upgrade Test Content",
            slug,
            "Body for upgrade migration test.",
            nameof(ContentType.Article),
            userId,
            nameof(ContentStatus.Published),
            3,
            1,
            DateTime.UtcNow);

        await context.Database.MigrateAsync();

        await using var verifyContext = await CreateContextAsync(connectionString);
        var user = await verifyContext.Users.SingleAsync(user => user.Id == userId);
        var content = await verifyContext.Contents.SingleAsync(item => item.Id == contentId);

        Assert.Equal(mobile, user.Mobile);
        Assert.Equal(slug, content.Slug.Value);
        Assert.Equal(ContentStatus.Published, content.Status);

        // AddContentCmsFieldsV1 backfill: existing published rows get published_at_utc = created_at.
        Assert.NotNull(content.PublishedAtUtc);

        var tables = await PostgreSqlDatabaseHelper.GetExistingModuleTablesAsync(connectionString);
        Assert.Contains("analytics_daily_metrics", tables);
        Assert.Contains("audit_records", tables);
        Assert.Contains("promptlab_prompts", tables);

        var migrationCount = await PostgreSqlDatabaseHelper.GetAppliedMigrationCountAsync(connectionString);
        Assert.Equal(PostgreSqlDatabaseHelper.ExpectedMigrationCount, migrationCount);
    }

    private static async Task<ApplicationDbContext> CreateContextAtMigrationAsync(
        string connectionString,
        string migrationId)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name))
            .Options;

        var context = new ApplicationDbContext(options);
        var migrator = context.Database.GetService<IMigrator>();
        await migrator.MigrateAsync(ToolboxMigrationId);
        return context;
    }
}
