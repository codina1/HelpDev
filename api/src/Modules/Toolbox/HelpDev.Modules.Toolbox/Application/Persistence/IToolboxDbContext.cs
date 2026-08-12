using HelpDev.Modules.Toolbox.Domain.Categories;
using HelpDev.Modules.Toolbox.Domain.Execution;
using HelpDev.Modules.Toolbox.Domain.Favorites;
using HelpDev.Modules.Toolbox.Domain.Tools;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Toolbox.Application.Persistence;

public interface IToolboxDbContext
{
    DbSet<ToolCategory> ToolCategories { get; }

    DbSet<ToolDefinition> ToolDefinitions { get; }

    DbSet<ToolFavorite> ToolFavorites { get; }

    DbSet<ToolExecutionRecord> ToolExecutionRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
