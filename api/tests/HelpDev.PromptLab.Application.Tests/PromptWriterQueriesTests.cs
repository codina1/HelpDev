using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Application.Prompts;
using HelpDev.Modules.PromptLab.Domain.AiModels;
using HelpDev.Modules.PromptLab.Domain.Categories;
using HelpDev.Modules.PromptLab.Domain.Favorites;
using HelpDev.Modules.PromptLab.Domain.Packs;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.Modules.PromptLab.Domain.Rendering;
using HelpDev.Modules.PromptLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.PromptLab.Application.Tests;

public sealed class PromptWriterQueriesTests
{
    private static readonly DateTime Now = new(2026, 8, 17, 10, 0, 0, DateTimeKind.Utc);
    private static readonly Guid AuthorId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid OtherAuthorId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public async Task List_returns_only_the_current_writers_prompts()
    {
        await using var db = CreateDb();
        var queries = new PromptWriterQueries(db);
        SeedCatalog(db);
        SeedPrompt(db, "mine-draft", AuthorId, PromptStatus.Draft);
        SeedPrompt(db, "mine-submitted", AuthorId, PromptStatus.Submitted);
        SeedPrompt(db, "theirs-draft", OtherAuthorId, PromptStatus.Draft);
        await db.SaveChangesAsync();

        var page = await queries.GetMyPromptsAsync(AuthorId, new WriterPromptFilter(null, 1, 20));

        Assert.Equal(2, page.Total);
        Assert.Equal(2, page.Items.Count);
        Assert.All(page.Items, item => Assert.DoesNotContain("theirs", item.Slug, StringComparison.Ordinal));
        Assert.Contains(page.Items, item => item.Status == nameof(PromptStatus.Draft));
        Assert.Contains(page.Items, item => item.Status == nameof(PromptStatus.Submitted));
    }

    [Fact]
    public async Task GetById_hides_other_writers_prompts()
    {
        await using var db = CreateDb();
        var queries = new PromptWriterQueries(db);
        SeedCatalog(db);
        var mine = SeedPrompt(db, "mine-draft", AuthorId, PromptStatus.Draft);
        var theirs = SeedPrompt(db, "theirs-draft", OtherAuthorId, PromptStatus.Draft);
        await db.SaveChangesAsync();

        var owned = await queries.GetMyByIdAsync(AuthorId, mine.Id);
        Assert.NotNull(owned);
        Assert.Equal("mine-draft", owned!.Slug);
        Assert.Equal("Review {{code}}", owned.Content);

        Assert.Null(await queries.GetMyByIdAsync(AuthorId, theirs.Id));
    }

    private static Prompt SeedPrompt(
        TestPromptLabDbContext db,
        string slug,
        Guid authorId,
        PromptStatus status)
    {
        var prompt = Prompt.Create(
            Guid.NewGuid(),
            TitleFromSlug(slug),
            slug,
            "Helps review code",
            "Review {{code}}",
            null,
            PromptMediaType.Text,
            db.ChatGpt.Id,
            db.Coding.Id,
            authorId,
            Now);

        if (status is PromptStatus.Submitted or PromptStatus.Approved or PromptStatus.Rejected)
        {
            prompt.Submit(authorId, Now.AddMinutes(1));
        }

        if (status is PromptStatus.Approved)
        {
            prompt.Approve(Now.AddMinutes(2));
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
        db.PromptCategories.Add(db.Coding);
        db.AiModels.Add(db.ChatGpt);
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
            ChatGpt = AiModel.Create(Guid.NewGuid(), "ChatGPT", "chatgpt", "OpenAI", "chatgpt", Now);
        }

        public PromptCategory Coding { get; }

        public AiModel ChatGpt { get; }

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
