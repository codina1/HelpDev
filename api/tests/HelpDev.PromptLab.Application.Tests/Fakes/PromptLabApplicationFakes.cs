using System.Text.Json;
using HelpDev.Modules.PromptLab.Application.Categories;
using HelpDev.Modules.PromptLab.Application.Favorites;
using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Application.Prompts;
using HelpDev.Modules.PromptLab.Application.Rendering;
using HelpDev.Modules.PromptLab.Domain.AiModels;
using HelpDev.Modules.PromptLab.Domain.Categories;
using HelpDev.Modules.PromptLab.Domain.Favorites;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.Modules.PromptLab.Domain.Rendering;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedKernel.Time;
using HelpDev.Testing.Auditing;
using Microsoft.Extensions.Logging.Abstractions;

namespace HelpDev.PromptLab.Application.Tests.Fakes;

internal sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public FakeDateTimeProvider(DateTime utcNow) =>
        UtcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);

    public DateTime UtcNow { get; private set; }

    public void Advance(TimeSpan delta) => UtcNow = UtcNow.Add(delta);
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCount { get; private set; }

    public CancellationToken LastToken { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        SaveChangesCount++;
        return Task.FromResult(1);
    }
}

internal sealed class FakePromptCategoryRepository : IPromptCategoryRepository
{
    private readonly List<PromptCategory> _items = [];

    public int AddCallCount { get; private set; }

    public CancellationToken LastToken { get; private set; }

    public IReadOnlyList<PromptCategory> Items => _items;

    public void Seed(PromptCategory category) => _items.Add(category);

    public Task<PromptCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        return Task.FromResult(_items.FirstOrDefault(item => item.Id == id));
    }

    public Task<PromptCategory?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        return Task.FromResult(_items.FirstOrDefault(item =>
            string.Equals(item.Slug.Value, slug, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        return Task.FromResult(_items.Any(item =>
            string.Equals(item.Slug.Value, slug, StringComparison.OrdinalIgnoreCase)));
    }

    public Task AddAsync(PromptCategory category, CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        AddCallCount++;
        _items.Add(category);
        return Task.CompletedTask;
    }
}

internal sealed class FakePromptCategoryQueries : IPromptCategoryQueries
{
    public IReadOnlyList<PromptCategoryAdminDto> All { get; set; } = [];

    public PromptCategoryAdminDto? ById { get; set; }

    public Task<IReadOnlyList<PromptCategoryAdminDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(All);

    public Task<PromptCategoryAdminDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(ById);
}

internal sealed class FakePromptDefinitionRepository : IPromptDefinitionRepository
{
    private readonly List<PromptDefinition> _items = [];

    public int AddCallCount { get; private set; }

    public CancellationToken LastToken { get; private set; }

    public IReadOnlyList<PromptDefinition> Items => _items;

    public void Seed(PromptDefinition prompt) => _items.Add(prompt);

    public Task<PromptDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        return Task.FromResult(_items.FirstOrDefault(item => item.Id == id));
    }

    public Task<PromptDefinition?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        return Task.FromResult(_items.FirstOrDefault(item =>
            string.Equals(item.Slug.Value, slug, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        return Task.FromResult(_items.Any(item =>
            string.Equals(item.Slug.Value, slug, StringComparison.OrdinalIgnoreCase)));
    }

    public Task AddAsync(PromptDefinition prompt, CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        AddCallCount++;
        _items.Add(prompt);
        return Task.CompletedTask;
    }
}

internal sealed class FakePromptDefinitionQueries : IPromptDefinitionQueries
{
    public PromptDefinitionAdminDto? ById { get; set; }

    public PromptDefinitionPageDto Page { get; set; } = new(1, 20, 0, []);

    public IReadOnlyList<PromptVersionAdminDto> Versions { get; set; } = [];

    public PromptVersionAdminDto? Version { get; set; }

    public Task<PromptDefinitionAdminDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(ById);

    public Task<PromptDefinitionPageDto> GetPageAsync(
        PromptDefinitionFilter filter,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Page);

    public Task<IReadOnlyList<PromptVersionAdminDto>> GetVersionsAsync(
        Guid promptId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Versions);

    public Task<PromptVersionAdminDto?> GetVersionAsync(
        Guid promptId,
        int versionNumber,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Version);
}

internal sealed class FakePromptFavoriteRepository : IPromptFavoriteRepository
{
    private readonly List<PromptFavorite> _items = [];

    public int AddCallCount { get; private set; }

    public int RemoveCallCount { get; private set; }

    public CancellationToken LastToken { get; private set; }

    public IReadOnlyList<PromptFavorite> Items => _items;

    public void Seed(PromptFavorite favorite) => _items.Add(favorite);

    public Task<PromptFavorite?> GetAsync(
        Guid userId,
        Guid promptDefinitionId,
        CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        return Task.FromResult(_items.FirstOrDefault(item =>
            item.UserId == userId && item.PromptDefinitionId == promptDefinitionId));
    }

    public Task AddAsync(PromptFavorite favorite, CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        AddCallCount++;
        _items.Add(favorite);
        return Task.CompletedTask;
    }

    public void Remove(PromptFavorite favorite)
    {
        RemoveCallCount++;
        _items.Remove(favorite);
    }
}

internal sealed class FakePromptFavoriteQueries : IPromptFavoriteQueries
{
    public IReadOnlyList<PromptFavoriteDto> Favorites { get; set; } = [];

    public Task<IReadOnlyList<PromptFavoriteDto>> GetUserFavoritesAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Favorites);
}

internal sealed class FakePromptRenderRecordRepository : IPromptRenderRecordRepository
{
    private readonly List<PromptRenderRecord> _items = [];

    public int AddCallCount { get; private set; }

    public CancellationToken LastToken { get; private set; }

    public IReadOnlyList<PromptRenderRecord> Items => _items;

    public Task AddAsync(PromptRenderRecord record, CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        AddCallCount++;
        _items.Add(record);
        return Task.CompletedTask;
    }
}

internal sealed class FakePromptRepository : IPromptRepository
{
    private readonly List<Prompt> _items = [];

    public int AddCallCount { get; private set; }

    public IReadOnlyList<Prompt> Items => _items;

    public void Seed(Prompt prompt) => _items.Add(prompt);

    public Task<Prompt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.FirstOrDefault(item => item.Id == id));

    public Task<bool> ExistsBySlugAsync(
        string slug,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.Any(item =>
            string.Equals(item.Slug.Value, slug, StringComparison.OrdinalIgnoreCase)
            && (!excludingId.HasValue || item.Id != excludingId.Value)));

    public Task AddAsync(Prompt prompt, CancellationToken cancellationToken = default)
    {
        AddCallCount++;
        _items.Add(prompt);
        return Task.CompletedTask;
    }
}

internal sealed class FakeAiModelRepository : IAiModelRepository
{
    private readonly List<AiModel> _items = [];

    public IReadOnlyList<AiModel> Items => _items;

    public void Seed(AiModel model) => _items.Add(model);

    public Task<AiModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.FirstOrDefault(item => item.Id == id));
}

internal static class ServiceFactory
{
    public static PromptCategoryService CreateCategoryService(
        FakePromptCategoryRepository repository,
        FakePromptCategoryQueries queries,
        FakeUnitOfWork unitOfWork,
        FakeDateTimeProvider clock,
        IAuditRecorder? auditRecorder = null,
        IAuditRequestContext? auditRequestContext = null) =>
        new(
            repository,
            queries,
            unitOfWork,
            clock,
            auditRecorder ?? new NoOpAuditRecorder(),
            auditRequestContext ?? new FakeAuditRequestContext(),
            NullLogger<PromptCategoryService>.Instance);

    public static PromptDefinitionService CreateDefinitionService(
        FakePromptDefinitionRepository repository,
        FakePromptCategoryRepository categoryRepository,
        FakePromptDefinitionQueries queries,
        FakeUnitOfWork unitOfWork,
        FakeDateTimeProvider clock,
        IPromptTemplateParser? parser = null,
        IAuditRecorder? auditRecorder = null,
        IAuditRequestContext? auditRequestContext = null) =>
        new(
            repository,
            categoryRepository,
            queries,
            parser ?? new PromptTemplateParser(),
            unitOfWork,
            clock,
            auditRecorder ?? new NoOpAuditRecorder(),
            auditRequestContext ?? new FakeAuditRequestContext(),
            NullLogger<PromptDefinitionService>.Instance);

    public static PromptRenderService CreateRenderService(
        FakePromptDefinitionRepository promptRepository,
        FakePromptCategoryRepository categoryRepository,
        FakePromptRenderRecordRepository renderRepository,
        FakeUnitOfWork unitOfWork,
        FakeDateTimeProvider clock,
        IPromptRenderer? renderer = null) =>
        new(
            promptRepository,
            categoryRepository,
            renderRepository,
            renderer ?? new PromptRenderer(),
            unitOfWork,
            clock,
            new HelpDev.Testing.Analytics.NoOpAnalyticsEventIngestor(),
            NullLogger<PromptRenderService>.Instance);

    public static PromptFavoriteService CreateFavoriteService(
        FakePromptFavoriteRepository favoriteRepository,
        FakePromptDefinitionRepository promptRepository,
        FakePromptFavoriteQueries queries,
        FakeUnitOfWork unitOfWork,
        FakeDateTimeProvider clock) =>
        new(
            favoriteRepository,
            promptRepository,
            queries,
            unitOfWork,
            clock,
            NullLogger<PromptFavoriteService>.Instance);

    public static PromptWriterService CreateWriterService(
        FakePromptRepository prompts,
        FakePromptCategoryRepository categories,
        FakeAiModelRepository aiModels,
        FakeUnitOfWork unitOfWork,
        FakeDateTimeProvider clock,
        IAuditRecorder? auditRecorder = null,
        IAuditRequestContext? auditRequestContext = null) =>
        new(
            prompts,
            categories,
            aiModels,
            unitOfWork,
            clock,
            auditRecorder ?? new NoOpAuditRecorder(),
            auditRequestContext ?? new FakeAuditRequestContext(),
            NullLogger<PromptWriterService>.Instance);

    public static PromptCategory CreateActiveCategory(DateTime utcNow, string slug = "coding") =>
        PromptCategory.Create(Guid.NewGuid(), "Coding", slug, null, null, 0, utcNow);

    public static PromptDefinition CreateDraftPrompt(
        DateTime utcNow,
        Guid categoryId,
        string slug = "code-review",
        bool allowHistory = false,
        bool requiresAuthentication = false) =>
        PromptDefinition.CreateDraft(
            Guid.NewGuid(),
            categoryId,
            "Code Review",
            slug,
            "Reviews code",
            null,
            PromptPurpose.CodeReview,
            requiresAuthentication ? PromptVisibility.Authenticated : PromptVisibility.Public,
            requiresAuthentication,
            allowHistory,
            displayOrder: 0,
            utcNow);

    public static PromptDefinition CreatePublishedPrompt(
        DateTime utcNow,
        Guid? categoryId = null,
        string slug = "code-review",
        bool allowHistory = false,
        bool requiresAuthentication = false,
        bool enabled = true)
    {
        var prompt = CreateDraftPrompt(
            utcNow,
            categoryId ?? Guid.NewGuid(),
            slug,
            allowHistory,
            requiresAuthentication);

        var versionId = Guid.NewGuid();
        var variable = PromptVariable.Create(
            Guid.NewGuid(),
            versionId,
            "code",
            "Code",
            null,
            PromptVariableType.MultilineText,
            true,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            0);

        prompt.RegisterVersion(
            versionId,
            "Review {{code}}",
            null,
            null,
            [variable],
            ["code"],
            utcNow);

        if (!enabled)
        {
            prompt.Disable(utcNow);
            return prompt;
        }

        prompt.PublishVersion(1, utcNow);
        return prompt;
    }

    public static Dictionary<string, JsonElement> RenderValues(string code)
    {
        var values = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase)
        {
            ["code"] = JsonSerializer.SerializeToElement(code),
        };
        return values;
    }
}
