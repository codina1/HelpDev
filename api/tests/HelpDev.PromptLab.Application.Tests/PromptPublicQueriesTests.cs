using HelpDev.Modules.PromptLab.Application;
using HelpDev.Modules.PromptLab.Application.Catalog;
using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Domain.AiModels;
using HelpDev.Modules.PromptLab.Domain.Categories;
using HelpDev.Modules.PromptLab.Domain.Favorites;
using HelpDev.Modules.PromptLab.Domain.Packs;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.Modules.PromptLab.Domain.Rendering;
using HelpDev.Modules.PromptLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.PromptLab.Application.Tests;

public sealed class PromptPublicQueriesTests
{
    private static readonly DateTime Now = new(2026, 8, 17, 8, 0, 0, DateTimeKind.Utc);
    private static readonly Guid AuthorId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public void Public_dtos_never_expose_status_or_unpublished_fields()
    {
        Assert.DoesNotContain(
            typeof(PublicPromptListItemDto).GetProperties().Select(property => property.Name),
            name => name.Contains("Status", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            typeof(PublicPromptDetailsDto).GetProperties().Select(property => property.Name),
            name => name.Contains("Status", StringComparison.OrdinalIgnoreCase));
        Assert.Null(typeof(PublicPromptListItemDto).GetProperty("Content"));
        Assert.NotNull(typeof(PublicPromptDetailsDto).GetProperty("Content"));
    }

    [Fact]
    public async Task List_returns_only_approved_prompts()
    {
        await using var db = CreateDb();
        var queries = new PromptPublicQueries(db);

        SeedCatalog(db);
        SeedPrompt(db, "draft-prompt", PromptStatus.Draft, db.Coding.Id, db.ChatGpt.Id);
        SeedPrompt(db, "submitted-prompt", PromptStatus.Submitted, db.Coding.Id, db.ChatGpt.Id);
        SeedPrompt(db, "rejected-prompt", PromptStatus.Rejected, db.Coding.Id, db.ChatGpt.Id);
        var approved = SeedPrompt(db, "approved-prompt", PromptStatus.Approved, db.Coding.Id, db.ChatGpt.Id);
        await db.SaveChangesAsync();

        var page = await queries.GetPromptsAsync(DefaultFilter());

        var item = Assert.Single(page.Items);
        Assert.Equal(approved.Slug.Value, item.Slug);
        Assert.Equal("Approved Prompt", item.Title);
        Assert.Equal("coding", item.Category.Slug);
        Assert.Equal("chatgpt", item.AiModel.Slug);
        Assert.DoesNotContain(page.Items, row => row.Slug.Contains("draft", StringComparison.Ordinal));
        Assert.DoesNotContain(page.Items, row => row.Slug.Contains("submitted", StringComparison.Ordinal));
        Assert.DoesNotContain(page.Items, row => row.Slug.Contains("rejected", StringComparison.Ordinal));
    }

    [Fact]
    public async Task List_hides_approved_prompts_in_inactive_category_or_model()
    {
        await using var db = CreateDb();
        var queries = new PromptPublicQueries(db);

        SeedCatalog(db);
        SeedPrompt(db, "inactive-category-prompt", PromptStatus.Approved, db.InactiveCategory.Id, db.ChatGpt.Id);
        SeedPrompt(db, "inactive-model-prompt", PromptStatus.Approved, db.Coding.Id, db.InactiveModel.Id);
        await db.SaveChangesAsync();

        var page = await queries.GetPromptsAsync(DefaultFilter());

        Assert.Empty(page.Items);
        Assert.Equal(0, page.Total);
    }

    [Fact]
    public async Task List_applies_category_model_media_search_and_popular_filters()
    {
        await using var db = CreateDb();
        var queries = new PromptPublicQueries(db);

        SeedCatalog(db);
        var image = SeedPrompt(
            db,
            "image-helper",
            PromptStatus.Approved,
            db.Writing.Id,
            db.Claude.Id,
            title: "Image helper",
            description: "Creates posters",
            mediaType: PromptMediaType.Image,
            publishedAt: Now.AddHours(-2));
        image.RecordView();
        image.RecordView();
        image.RecordCopy();

        SeedPrompt(
            db,
            "text-review",
            PromptStatus.Approved,
            db.Coding.Id,
            db.ChatGpt.Id,
            title: "Text review",
            description: "Reviews source code",
            mediaType: PromptMediaType.Text,
            publishedAt: Now.AddHours(-1));
        await db.SaveChangesAsync();

        var byCategory = await queries.GetPromptsAsync(DefaultFilter(category: "writing"));
        Assert.Equal("image-helper", Assert.Single(byCategory.Items).Slug);

        var byModel = await queries.GetPromptsAsync(DefaultFilter(aiModel: "chatgpt"));
        Assert.Equal("text-review", Assert.Single(byModel.Items).Slug);

        var byMedia = await queries.GetPromptsAsync(DefaultFilter(mediaType: "image"));
        Assert.Equal("image-helper", Assert.Single(byMedia.Items).Slug);

        var bySearch = await queries.GetPromptsAsync(DefaultFilter(search: "posters"));
        Assert.Equal("image-helper", Assert.Single(bySearch.Items).Slug);

        var unpublishedMatch = SeedPrompt(
            db,
            "hidden-posters",
            PromptStatus.Draft,
            db.Writing.Id,
            db.Claude.Id,
            title: "Hidden posters",
            description: "Creates posters in draft");
        await db.SaveChangesAsync();
        _ = unpublishedMatch;

        var searchStillPublic = await queries.GetPromptsAsync(DefaultFilter(search: "posters"));
        Assert.Equal("image-helper", Assert.Single(searchStillPublic.Items).Slug);

        var popular = await queries.GetPromptsAsync(DefaultFilter(popular: true));
        Assert.Equal(["image-helper", "text-review"], popular.Items.Select(item => item.Slug).ToArray());
    }

    [Fact]
    public async Task GetBySlug_returns_approved_content_and_hides_unpublished()
    {
        await using var db = CreateDb();
        var queries = new PromptPublicQueries(db);

        SeedCatalog(db);
        SeedPrompt(
            db,
            "public-helper",
            PromptStatus.Approved,
            db.Coding.Id,
            db.ChatGpt.Id,
            content: "public body");
        SeedPrompt(
            db,
            "secret-draft",
            PromptStatus.Draft,
            db.Coding.Id,
            db.ChatGpt.Id,
            content: "never leak this");
        await db.SaveChangesAsync();

        var details = await queries.GetBySlugAsync("public-helper");
        Assert.NotNull(details);
        Assert.Equal("public body", details!.Content);
        Assert.Equal("coding", details.Category.Slug);
        Assert.Equal("chatgpt", details.AiModel.Slug);

        Assert.Null(await queries.GetBySlugAsync("secret-draft"));
        Assert.Null(await queries.GetBySlugAsync("missing-slug"));
    }

    [Fact]
    public async Task Invalid_paging_is_rejected()
    {
        await using var db = CreateDb();
        var queries = new PromptPublicQueries(db);

        var ex = await Assert.ThrowsAsync<PromptLabException>(
            () => queries.GetPromptsAsync(DefaultFilter() with { Page = 0 }));
        Assert.Equal(PromptLabApplicationErrorCodes.PaginationInvalid, ex.Code);
    }

    private static PublicPromptFilter DefaultFilter(
        string? category = null,
        string? aiModel = null,
        string? mediaType = null,
        string? search = null,
        bool popular = false) =>
        new(category, aiModel, mediaType, search, popular, 1, 20);

    private static Prompt SeedPrompt(
        TestPromptLabDbContext db,
        string slug,
        PromptStatus status,
        Guid categoryId,
        Guid aiModelId,
        string? title = null,
        string? description = "A public description",
        string content = "Prompt body",
        PromptMediaType mediaType = PromptMediaType.Text,
        DateTime? publishedAt = null)
    {
        var prompt = Prompt.Create(
            Guid.NewGuid(),
            title ?? TitleFromSlug(slug),
            slug,
            description,
            content,
            coverImage: null,
            mediaType,
            aiModelId,
            categoryId,
            AuthorId,
            Now);

        if (status is PromptStatus.Submitted or PromptStatus.Approved or PromptStatus.Rejected)
        {
            prompt.Submit(AuthorId, Now.AddMinutes(1));
        }

        if (status is PromptStatus.Approved)
        {
            prompt.Approve(publishedAt ?? Now.AddMinutes(2));
        }

        if (status is PromptStatus.Rejected)
        {
            prompt.Reject(Now.AddMinutes(2));
        }

        db.Prompts.Add(prompt);
        return prompt;
    }

    private static string TitleFromSlug(string slug) =>
        string.Join(' ', slug.Split('-', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));

    private static void SeedCatalog(TestPromptLabDbContext db)
    {
        db.PromptCategories.AddRange(db.Coding, db.Writing, db.InactiveCategory);
        db.AiModels.AddRange(db.ChatGpt, db.Claude, db.InactiveModel);
    }

    private static TestPromptLabDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<TestPromptLabDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new TestPromptLabDbContext(options);
    }

    private sealed class TestPromptLabDbContext : DbContext, IPromptLabDbContext
    {
        public TestPromptLabDbContext(DbContextOptions<TestPromptLabDbContext> options)
            : base(options)
        {
            Coding = PromptCategory.Create(Guid.NewGuid(), "Coding", "coding", null, "code", 0, Now);
            Writing = PromptCategory.Create(Guid.NewGuid(), "Writing", "writing", null, "pen", 1, Now);
            InactiveCategory = PromptCategory.Create(Guid.NewGuid(), "Hidden", "hidden", null, "x", 2, Now);
            InactiveCategory.Deactivate(Now);

            ChatGpt = AiModel.Create(Guid.NewGuid(), "ChatGPT", "chatgpt", "OpenAI", "chatgpt", Now);
            Claude = AiModel.Create(Guid.NewGuid(), "Claude", "claude", "Anthropic", "claude", Now);
            InactiveModel = AiModel.Create(Guid.NewGuid(), "Retired", "retired", "None", "retired", Now);
            InactiveModel.Deactivate(Now);
        }

        public PromptCategory Coding { get; }

        public PromptCategory Writing { get; }

        public PromptCategory InactiveCategory { get; }

        public AiModel ChatGpt { get; }

        public AiModel Claude { get; }

        public AiModel InactiveModel { get; }

        public DbSet<PromptCategory> PromptCategories => Set<PromptCategory>();

        public DbSet<PromptDefinition> PromptDefinitions => Set<PromptDefinition>();

        public DbSet<Prompt> Prompts => Set<Prompt>();

        public DbSet<AiModel> AiModels => Set<AiModel>();

        public DbSet<PromptPack> PromptPacks => Set<PromptPack>();

        public DbSet<PromptPackItem> PromptPackItems => Set<PromptPackItem>();

        public DbSet<PromptFavorite> PromptFavorites => Set<PromptFavorite>();

        public DbSet<PromptRenderRecord> PromptRenderRecords => Set<PromptRenderRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PromptConfiguration());
            modelBuilder.ApplyConfiguration(new PromptCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new AiModelConfiguration());
            modelBuilder.Entity<PromptCategory>().Ignore(category => category.DomainEvents);
            modelBuilder.Entity<PromptCategory>().Ignore(category => category.HasDomainEvents);
            modelBuilder.Ignore<PromptDefinition>();
            modelBuilder.Ignore<PromptPack>();
            modelBuilder.Ignore<PromptPackItem>();
            modelBuilder.Ignore<PromptFavorite>();
            modelBuilder.Ignore<PromptRenderRecord>();
        }
    }
}
