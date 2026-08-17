using HelpDev.Modules.PromptLab.Application;
using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Application.Prompts;
using HelpDev.Modules.PromptLab.Domain;
using HelpDev.Modules.PromptLab.Domain.AiModels;
using HelpDev.Modules.PromptLab.Domain.Categories;
using HelpDev.Modules.PromptLab.Domain.Favorites;
using HelpDev.Modules.PromptLab.Domain.Packs;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.Modules.PromptLab.Domain.Rendering;
using HelpDev.Modules.PromptLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.PromptLab.Application.Tests;

public sealed class PromptAdminReviewQueriesTests
{
    private static readonly DateTime Now = new(2026, 8, 17, 10, 0, 0, DateTimeKind.Utc);
    private static readonly Guid AuthorId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public async Task Pending_list_returns_submitted_title_author_category_and_preview()
    {
        await using var db = CreateDb();
        var queries = new PromptAdminReviewQueries(db);
        SeedCatalog(db);
        SeedPrompt(db, "draft-helper", PromptStatus.Draft);
        var pending = SeedPrompt(
            db,
            "pending-helper",
            PromptStatus.Submitted,
            new string('A', PromptLabLimits.AdminPromptPreviewLength + 40));
        SeedPrompt(db, "live-helper", PromptStatus.Approved);
        await db.SaveChangesAsync();

        var page = await queries.GetPromptsAsync(new AdminPromptReviewFilter("Submitted", 1, 20));

        var item = Assert.Single(page.Items);
        Assert.Equal(pending.Id, item.Id);
        Assert.Equal("Pending Helper", item.Title);
        Assert.Equal(AuthorId, item.AuthorId);
        Assert.Equal("Coding", item.CategoryName);
        Assert.Equal(PromptLabLimits.AdminPromptPreviewLength + 1, item.Preview.Length);
        Assert.EndsWith("…", item.Preview, StringComparison.Ordinal);
        Assert.Equal(nameof(PromptStatus.Submitted), item.Status);
    }

    [Fact]
    public async Task Rejected_list_includes_reason_and_skips_other_statuses()
    {
        await using var db = CreateDb();
        var queries = new PromptAdminReviewQueries(db);
        SeedCatalog(db);
        SeedPrompt(db, "pending-helper", PromptStatus.Submitted);
        SeedPrompt(db, "rejected-helper", PromptStatus.Rejected, reason: "عنوان مبهم است.");
        await db.SaveChangesAsync();

        var page = await queries.GetPromptsAsync(new AdminPromptReviewFilter("Rejected", 1, 20));

        var item = Assert.Single(page.Items);
        Assert.Equal("rejected-helper", item.Slug);
        Assert.Equal("عنوان مبهم است.", item.RejectionReason);
        Assert.Equal(nameof(PromptStatus.Rejected), item.Status);
    }

    [Fact]
    public async Task Invalid_review_status_is_rejected()
    {
        await using var db = CreateDb();
        var queries = new PromptAdminReviewQueries(db);

        var ex = await Assert.ThrowsAsync<PromptLabException>(
            () => queries.GetPromptsAsync(new AdminPromptReviewFilter("Draft", 1, 20)));
        Assert.Equal(PromptLabApplicationErrorCodes.PromptStatusInvalid, ex.Code);
    }

    private static Prompt SeedPrompt(
        TestPromptLabDbContext db,
        string slug,
        PromptStatus status,
        string? content = null,
        string? reason = null)
    {
        var prompt = Prompt.Create(
            Guid.NewGuid(),
            TitleFromSlug(slug),
            slug,
            "Helps review code",
            content ?? "Review {{code}}",
            null,
            PromptMediaType.Text,
            db.ChatGpt.Id,
            db.Coding.Id,
            AuthorId,
            Now);

        if (status is PromptStatus.Submitted or PromptStatus.Approved or PromptStatus.Rejected)
        {
            prompt.Submit(AuthorId, Now.AddMinutes(1));
        }

        if (status is PromptStatus.Approved)
        {
            prompt.Approve(Now.AddMinutes(2));
        }

        if (status is PromptStatus.Rejected)
        {
            prompt.Reject(Now.AddMinutes(2), reason);
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
