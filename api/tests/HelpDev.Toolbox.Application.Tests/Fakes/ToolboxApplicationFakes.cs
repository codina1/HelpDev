using System.Text.Json;
using HelpDev.Modules.Toolbox.Application.Categories;
using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Application.Favorites;
using HelpDev.Modules.Toolbox.Application.Persistence;
using HelpDev.Modules.Toolbox.Application.Tools;
using HelpDev.Modules.Toolbox.Domain.Categories;
using HelpDev.Modules.Toolbox.Domain.Execution;
using HelpDev.Modules.Toolbox.Domain.Favorites;
using HelpDev.Modules.Toolbox.Domain.Tools;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedKernel.Time;
using HelpDev.Testing.Auditing;
using Microsoft.Extensions.Logging.Abstractions;

namespace HelpDev.Toolbox.Application.Tests.Fakes;

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

internal sealed class FakeToolCategoryRepository : IToolCategoryRepository
{
    private readonly List<ToolCategory> _items = [];

    public int AddCallCount { get; private set; }

    public CancellationToken LastToken { get; private set; }

    public IReadOnlyList<ToolCategory> Items => _items;

    public void Seed(ToolCategory category) => _items.Add(category);

    public Task<ToolCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        return Task.FromResult(_items.FirstOrDefault(item => item.Id == id));
    }

    public Task<ToolCategory?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
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

    public Task AddAsync(ToolCategory category, CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        AddCallCount++;
        _items.Add(category);
        return Task.CompletedTask;
    }
}

internal sealed class FakeToolCategoryQueries : IToolCategoryQueries
{
    public IReadOnlyList<ToolCategoryAdminDto> All { get; set; } = [];

    public ToolCategoryAdminDto? ById { get; set; }

    public Task<IReadOnlyList<ToolCategoryAdminDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(All);

    public Task<ToolCategoryAdminDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(ById);
}

internal sealed class FakeToolDefinitionRepository : IToolDefinitionRepository
{
    private readonly List<ToolDefinition> _items = [];

    public int AddCallCount { get; private set; }

    public CancellationToken LastToken { get; private set; }

    public IReadOnlyList<ToolDefinition> Items => _items;

    public void Seed(ToolDefinition tool) => _items.Add(tool);

    public Task<ToolDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        return Task.FromResult(_items.FirstOrDefault(item => item.Id == id));
    }

    public Task<ToolDefinition?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
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

    public Task AddAsync(ToolDefinition tool, CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        AddCallCount++;
        _items.Add(tool);
        return Task.CompletedTask;
    }
}

internal sealed class FakeToolDefinitionQueries : IToolDefinitionQueries
{
    public ToolDefinitionAdminDto? ById { get; set; }

    public ToolDefinitionPageDto Page { get; set; } = new(1, 20, 0, []);

    public Task<ToolDefinitionAdminDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(ById);

    public Task<ToolDefinitionPageDto> GetPageAsync(
        ToolDefinitionFilter filter,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Page);
}

internal sealed class FakeToolFavoriteRepository : IToolFavoriteRepository
{
    private readonly List<ToolFavorite> _items = [];

    public int AddCallCount { get; private set; }

    public int RemoveCallCount { get; private set; }

    public CancellationToken LastToken { get; private set; }

    public IReadOnlyList<ToolFavorite> Items => _items;

    public void Seed(ToolFavorite favorite) => _items.Add(favorite);

    public Task<ToolFavorite?> GetAsync(Guid userId, Guid toolId, CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        return Task.FromResult(_items.FirstOrDefault(item => item.UserId == userId && item.ToolId == toolId));
    }

    public Task AddAsync(ToolFavorite favorite, CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        AddCallCount++;
        _items.Add(favorite);
        return Task.CompletedTask;
    }

    public void Remove(ToolFavorite favorite)
    {
        RemoveCallCount++;
        _items.Remove(favorite);
    }
}

internal sealed class FakeToolFavoriteQueries : IToolFavoriteQueries
{
    public IReadOnlyList<ToolFavoriteDto> Favorites { get; set; } = [];

    public Task<IReadOnlyList<ToolFavoriteDto>> GetUserFavoritesAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Favorites);
}

internal sealed class FakeToolExecutionRecordRepository : IToolExecutionRecordRepository
{
    private readonly List<ToolExecutionRecord> _items = [];

    public int AddCallCount { get; private set; }

    public CancellationToken LastToken { get; private set; }

    public IReadOnlyList<ToolExecutionRecord> Items => _items;

    public Task AddAsync(ToolExecutionRecord record, CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        AddCallCount++;
        _items.Add(record);
        return Task.CompletedTask;
    }
}

internal sealed class StubToolExecutor : IToolExecutor
{
    public StubToolExecutor(ToolType type = ToolType.TextStatistics) => Type = type;

    public ToolType Type { get; }

    public CancellationToken LastToken { get; private set; }

    public Task<ToolExecutionOutput> ExecuteAsync(
        ToolExecutionInput input,
        CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        var payload = JsonSerializer.SerializeToElement(new { ok = true });
        return Task.FromResult(new ToolExecutionOutput(payload));
    }
}

internal sealed class FakeExecutorRegistry : IToolExecutorRegistry
{
    private readonly IToolExecutor _executor;

    public FakeExecutorRegistry(IToolExecutor executor) => _executor = executor;

    public IToolExecutor GetRequired(ToolType type) => _executor;
}

internal static class ServiceFactory
{
    public const string DefaultSchema = """{"type":"object","properties":{"text":{"type":"string"}}}""";

    public static ToolCategoryService CreateCategoryService(
        FakeToolCategoryRepository repository,
        FakeToolCategoryQueries queries,
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
            NullLogger<ToolCategoryService>.Instance);

    public static ToolDefinitionService CreateToolService(
        FakeToolDefinitionRepository repository,
        FakeToolCategoryRepository categoryRepository,
        FakeToolDefinitionQueries queries,
        FakeUnitOfWork unitOfWork,
        FakeDateTimeProvider clock,
        IAuditRecorder? auditRecorder = null,
        IAuditRequestContext? auditRequestContext = null) =>
        new(
            repository,
            categoryRepository,
            queries,
            unitOfWork,
            clock,
            auditRecorder ?? new NoOpAuditRecorder(),
            auditRequestContext ?? new FakeAuditRequestContext(),
            NullLogger<ToolDefinitionService>.Instance);

    public static ToolExecutionService CreateExecutionService(
        FakeToolDefinitionRepository toolRepository,
        FakeToolExecutionRecordRepository executionRepository,
        FakeExecutorRegistry executorRegistry,
        FakeUnitOfWork unitOfWork,
        FakeDateTimeProvider clock) =>
        new(toolRepository, executionRepository, executorRegistry, unitOfWork, clock, new HelpDev.Testing.Analytics.NoOpAnalyticsEventIngestor(), NullLogger<ToolExecutionService>.Instance);

    public static ToolFavoriteService CreateFavoriteService(
        FakeToolFavoriteRepository favoriteRepository,
        FakeToolDefinitionRepository toolRepository,
        FakeToolFavoriteQueries queries,
        FakeUnitOfWork unitOfWork,
        FakeDateTimeProvider clock) =>
        new(favoriteRepository, toolRepository, queries, unitOfWork, clock, NullLogger<ToolFavoriteService>.Instance);

    public static ToolDefinition CreatePublishedTool(
        DateTime utcNow,
        Guid? categoryId = null,
        string slug = "text-stats",
        bool allowHistory = false,
        bool requiresAuthentication = false,
        bool enabled = true)
    {
        var tool = ToolDefinition.CreateDraft(
            Guid.NewGuid(),
            categoryId ?? Guid.NewGuid(),
            "Text Stats",
            slug,
            "Counts text",
            null,
            ToolType.TextStatistics,
            DefaultSchema,
            null,
            requiresAuthentication,
            allowHistory,
            displayOrder: 0,
            utcNow);

        if (!enabled)
        {
            tool.Disable(utcNow);
            return tool;
        }

        tool.Publish(utcNow);
        return tool;
    }
}
