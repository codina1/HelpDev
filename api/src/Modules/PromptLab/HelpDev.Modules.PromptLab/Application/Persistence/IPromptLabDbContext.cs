using HelpDev.Modules.PromptLab.Domain.Categories;
using HelpDev.Modules.PromptLab.Domain.Favorites;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.Modules.PromptLab.Domain.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.PromptLab.Application.Persistence;

public interface IPromptLabDbContext
{
    DbSet<PromptCategory> PromptCategories { get; }

    DbSet<PromptDefinition> PromptDefinitions { get; }

    DbSet<PromptFavorite> PromptFavorites { get; }

    DbSet<PromptRenderRecord> PromptRenderRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
