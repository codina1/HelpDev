using HelpDev.Infrastructure.Persistence;
using HelpDev.Modules.PromptLab.Domain.AiModels;
using HelpDev.Modules.PromptLab.Domain.Packs;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

namespace HelpDev.Infrastructure.Tests;

public sealed class PromptLabPersistenceModelTests
{
    [Fact]
    public void Prompt_maps_required_indexes_and_relationships()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(Prompt));
        Assert.NotNull(entity);
        Assert.Equal("promptlab_library_prompts", entity!.GetTableName());

        var indexed = entity.GetIndexes()
            .SelectMany(index => index.Properties.Select(property => property.Name))
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("Slug", indexed);
        Assert.Contains("Status", indexed);
        Assert.Contains("CategoryId", indexed);
        Assert.Contains("AiModelId", indexed);

        Assert.Contains(
            entity.GetForeignKeys(),
            key => key.Properties.Any(property => property.Name == "CategoryId"));
        Assert.Contains(
            entity.GetForeignKeys(),
            key => key.Properties.Any(property => property.Name == "AiModelId")
                && key.PrincipalEntityType.ClrType == typeof(AiModel));

        var rejection = entity.FindProperty(nameof(Prompt.RejectionReason));
        Assert.NotNull(rejection);
        Assert.Equal("rejection_reason", rejection!.GetColumnName());
        Assert.Equal(2000, rejection.GetMaxLength());
    }

    [Fact]
    public void Pack_items_preserve_order_relationship()
    {
        using var context = CreateContext();
        var pack = context.Model.FindEntityType(typeof(PromptPack));
        var item = context.Model.FindEntityType(typeof(PromptPackItem));
        Assert.NotNull(pack);
        Assert.NotNull(item);
        Assert.Equal("promptlab_packs", pack!.GetTableName());
        Assert.Equal("promptlab_pack_items", item!.GetTableName());

        Assert.Contains(
            item.GetIndexes(),
            index => index.IsUnique
                && index.Properties.Select(property => property.Name).SequenceEqual(["PackId", "Order"]));
        Assert.Contains(
            item.GetForeignKeys(),
            key => key.Properties.Any(property => property.Name == "PromptId")
                && key.PrincipalEntityType.ClrType == typeof(Prompt));
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=helpdev_model_probe;Username=helpdev;Password=helpdev",
                npgsql => npgsql.UseVector())
            .Options;

        return new ApplicationDbContext(options);
    }
}
