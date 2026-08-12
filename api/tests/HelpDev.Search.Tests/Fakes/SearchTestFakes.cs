using HelpDev.Modules.Search.Application.Contracts;
using HelpDev.Modules.Search.Application.Persistence;
using HelpDev.Modules.Search.Application.Reindex;
using HelpDev.Modules.Search.Domain;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedKernel.Time;

namespace HelpDev.Search.Tests.Fakes;

internal sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public FakeDateTimeProvider(DateTime utcNow) =>
        UtcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);

    public DateTime UtcNow { get; private set; }

    public void SetUtcNow(DateTime utcNow) =>
        UtcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCount { get; private set; }

    public CancellationToken LastCancellationToken { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        LastCancellationToken = cancellationToken;
        SaveChangesCount++;
        return Task.FromResult(1);
    }
}

internal sealed class FakeSearchDocumentRepository : ISearchDocumentRepository
{
    private readonly List<SearchDocument> _documents = [];

    public IReadOnlyList<SearchDocument> Documents => _documents;

    public CancellationToken LastCancellationToken { get; private set; }

    public List<(string SourceType, Guid? After, int Take)> ListSourceIdCalls { get; } = [];

    public Task<SearchDocument?> GetBySourceAsync(
        string sourceType,
        Guid sourceId,
        CancellationToken cancellationToken = default)
    {
        LastCancellationToken = cancellationToken;
        return Task.FromResult(
            _documents.FirstOrDefault(document =>
                document.SourceType == sourceType && document.SourceId == sourceId));
    }

    public Task<IReadOnlyList<Guid>> ListSourceIdsByTypeAsync(
        string sourceType,
        Guid? afterSourceId,
        int take,
        CancellationToken cancellationToken = default)
    {
        LastCancellationToken = cancellationToken;
        ListSourceIdCalls.Add((sourceType, afterSourceId, take));

        var query = _documents
            .Where(document => document.SourceType == sourceType)
            .OrderBy(document => document.SourceId)
            .Select(document => document.SourceId)
            .AsEnumerable();

        if (afterSourceId.HasValue)
        {
            query = query.Where(id => id.CompareTo(afterSourceId.Value) > 0);
        }

        IReadOnlyList<Guid> page = query.Take(take).ToList();
        return Task.FromResult(page);
    }

    public Task AddAsync(SearchDocument document, CancellationToken cancellationToken = default)
    {
        LastCancellationToken = cancellationToken;
        if (_documents.Any(existing =>
                existing.SourceType == document.SourceType && existing.SourceId == document.SourceId))
        {
            throw new InvalidOperationException(
                "Unique (SourceType, SourceId) invariant violated.");
        }

        _documents.Add(document);
        return Task.CompletedTask;
    }

    public void Remove(SearchDocument document)
    {
        _documents.Remove(document);
    }

    public void Seed(SearchDocument document) => _documents.Add(document);
}

internal sealed class FakeContentSearchSource : IContentSearchSource
{
    public SearchSourceDocument? Document { get; set; }

    public Dictionary<Guid, SearchSourceDocument?> DocumentsById { get; } = new();

    public List<SearchSourceDocument> PublishedBatch { get; set; } = [];

    public Exception? ExceptionToThrow { get; set; }

    public Guid? LastContentId { get; private set; }

    public CancellationToken LastCancellationToken { get; private set; }

    public int CallCount { get; private set; }

    public List<(Guid? After, int Take)> BatchCalls { get; } = [];

    public Task<SearchSourceDocument?> GetByIdAsync(
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastContentId = contentId;
        LastCancellationToken = cancellationToken;
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        if (DocumentsById.TryGetValue(contentId, out var mapped))
        {
            return Task.FromResult(mapped);
        }

        return Task.FromResult(
            Document is not null && Document.SourceId == contentId ? Document : null);
    }

    public Task<IReadOnlyList<SearchSourceDocument>> GetPublishedBatchAsync(
        Guid? afterSourceId,
        int take,
        CancellationToken cancellationToken = default)
    {
        LastCancellationToken = cancellationToken;
        BatchCalls.Add((afterSourceId, take));

        var query = PublishedBatch.OrderBy(document => document.SourceId).AsEnumerable();
        if (afterSourceId.HasValue)
        {
            query = query.Where(document => document.SourceId.CompareTo(afterSourceId.Value) > 0);
        }

        IReadOnlyList<SearchSourceDocument> page = query.Take(take).ToList();
        return Task.FromResult(page);
    }
}

internal sealed class FakeCourseSearchSource : ICourseSearchSource
{
    public SearchSourceDocument? Document { get; set; }

    public Dictionary<Guid, SearchSourceDocument?> DocumentsById { get; } = new();

    public List<SearchSourceDocument> PublishedBatch { get; set; } = [];

    public Exception? ExceptionToThrow { get; set; }

    public Guid? LastCourseId { get; private set; }

    public CancellationToken LastCancellationToken { get; private set; }

    public List<(Guid? After, int Take)> BatchCalls { get; } = [];

    public Task<SearchSourceDocument?> GetByIdAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        LastCourseId = courseId;
        LastCancellationToken = cancellationToken;
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        if (DocumentsById.TryGetValue(courseId, out var mapped))
        {
            return Task.FromResult(mapped);
        }

        return Task.FromResult(
            Document is not null && Document.SourceId == courseId ? Document : null);
    }

    public Task<IReadOnlyList<SearchSourceDocument>> GetPublishedBatchAsync(
        Guid? afterSourceId,
        int take,
        CancellationToken cancellationToken = default)
    {
        LastCancellationToken = cancellationToken;
        BatchCalls.Add((afterSourceId, take));

        var query = PublishedBatch.OrderBy(document => document.SourceId).AsEnumerable();
        if (afterSourceId.HasValue)
        {
            query = query.Where(document => document.SourceId.CompareTo(afterSourceId.Value) > 0);
        }

        IReadOnlyList<SearchSourceDocument> page = query.Take(take).ToList();
        return Task.FromResult(page);
    }
}

internal sealed class FakeLessonSearchSource : ILessonSearchSource
{
    public Task<SearchSourceDocument?> GetByIdAsync(Guid lessonId, CancellationToken cancellationToken = default) =>
        Task.FromResult<SearchSourceDocument?>(null);

    public Task<IReadOnlyList<Guid>> ListIdsByCourseAsync(Guid courseId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Guid>>([]);

    public Task<IReadOnlyList<SearchSourceDocument>> GetPublishedBatchAsync(
        Guid? afterSourceId,
        int take,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<SearchSourceDocument>>([]);
}

internal sealed class FakeToolSearchSource : IToolSearchSource
{
    public Task<SearchSourceDocument?> GetByIdAsync(Guid toolId, CancellationToken cancellationToken = default) =>
        Task.FromResult<SearchSourceDocument?>(null);

    public Task<IReadOnlyList<SearchSourceDocument>> GetPublishedBatchAsync(
        Guid? afterSourceId,
        int take,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<SearchSourceDocument>>([]);
}

internal sealed class FakePromptSearchSource : IPromptSearchSource
{
    public Task<SearchSourceDocument?> GetByIdAsync(Guid promptId, CancellationToken cancellationToken = default) =>
        Task.FromResult<SearchSourceDocument?>(null);

    public Task<IReadOnlyList<SearchSourceDocument>> GetPublishedBatchAsync(
        Guid? afterSourceId,
        int take,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<SearchSourceDocument>>([]);
}

internal sealed class FakeSearchReindexLock : ISearchReindexLock
{
    public bool AcquireSucceeded { get; set; } = true;

    public int AcquireCallCount { get; private set; }

    public int ReleaseCount { get; private set; }

    public bool IsHeld { get; private set; }

    public CancellationToken LastCancellationToken { get; private set; }

    public Task<IAsyncDisposable?> TryAcquireAsync(CancellationToken cancellationToken = default)
    {
        AcquireCallCount++;
        LastCancellationToken = cancellationToken;
        if (!AcquireSucceeded || IsHeld)
        {
            return Task.FromResult<IAsyncDisposable?>(null);
        }

        IsHeld = true;
        return Task.FromResult<IAsyncDisposable?>(new Lease(this));
    }

    private sealed class Lease : IAsyncDisposable
    {
        private readonly FakeSearchReindexLock _owner;

        public Lease(FakeSearchReindexLock owner) => _owner = owner;

        public ValueTask DisposeAsync()
        {
            _owner.IsHeld = false;
            _owner.ReleaseCount++;
            return ValueTask.CompletedTask;
        }
    }
}

