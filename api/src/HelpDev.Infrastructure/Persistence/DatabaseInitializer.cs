using HelpDev.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HelpDev.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task MigrateOnlyAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
    }

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DatabaseInitializer));
        var context = services.GetRequiredService<ApplicationDbContext>();

        try
        {
            await context.Database.MigrateAsync();
            await ApplicationDbContextSeed.SeedAsync(context, logger);
            await ApplicationDbContextSeed.EnsurePrimaryAdminAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database migration or seeding failed. API will start without seeded data.");
        }
    }
}
