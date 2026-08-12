using HelpDev.Modules.Search.Application.Contracts;
using HelpDev.Modules.Search.Application.Indexing;
using HelpDev.Modules.Search.Application.Reindex;
using HelpDev.Modules.Search.Domain;
using HelpDev.Search.Tests.Fakes;

namespace HelpDev.Search.Tests;

public sealed class SearchReindexServiceTests
{
    private readonly FakeContentSearchSource _contentSource = new();
    private readonly FakeCourseSearchSource _courseSource = new();
    private readonly FakeSearchDocumentRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeSearchReindexLock _lock = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 7, 19, 18, 0, 0, DateTimeKind.Utc));

    private SearchReindexService CreateSut() =>
        new(
            _contentSource,
            _courseSource,
            new SearchProjectionService(_repository, _unitOfWork, _clock),
            _repository,
            _unitOfWork,
            _lock,
            _clock);

    [Fact]
    public async Task Content_only_reindex_creates_documents_in_batches()
    {
        var id1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var id2 = Guid.Parse("00000000-0000-0000-0000-000000000002");
        _contentSource.PublishedBatch =
        [
            Source(SearchSourceTypes.Content, id1, "A", "a"),
            Source(SearchSourceTypes.Content, id2, "B", "b"),
        ];
        foreach (var item in _contentSource.PublishedBatch)
        {
            _contentSource.DocumentsById[item.SourceId] = item;
        }

        var result = await CreateSut().ReindexAsync(
            new SearchReindexRequest(SearchSourceTypes.Content, 10));

        Assert.Equal(2, result.Scanned);
        Assert.Equal(2, result.Created);
        Assert.Equal(0, result.Updated);
        Assert.Equal(2, _repository.Documents.Count);
        Assert.All(_repository.Documents, document => Assert.NotEqual(Guid.Empty, document.LastEventId));
        Assert.True(_unitOfWork.SaveChangesCount >= 1);
        Assert.Equal(1, _lock.ReleaseCount);
    }

    [Fact]
    public async Task Course_only_reindex_creates_documents()
    {
        var courseId = Guid.Parse("00000000-0000-0000-0000-000000000010");
        _courseSource.PublishedBatch = [Source(SearchSourceTypes.Course, courseId, "C#", "csharp")];
        _courseSource.DocumentsById[courseId] = _courseSource.PublishedBatch[0];

        var result = await CreateSut().ReindexAsync(
            new SearchReindexRequest(SearchSourceTypes.Course, 10));

        Assert.Equal(1, result.Scanned);
        Assert.Equal(1, result.Created);
        Assert.Equal(SearchSourceTypes.Course, Assert.Single(_repository.Documents).SourceType);
    }

    [Fact]
    public async Task Full_reindex_processes_content_and_course()
    {
        var contentId = Guid.Parse("00000000-0000-0000-0000-000000000021");
        var courseId = Guid.Parse("00000000-0000-0000-0000-000000000022");
        _contentSource.PublishedBatch = [Source(SearchSourceTypes.Content, contentId, "Post", "post")];
        _courseSource.PublishedBatch = [Source(SearchSourceTypes.Course, courseId, "Course", "course")];
        _contentSource.DocumentsById[contentId] = _contentSource.PublishedBatch[0];
        _courseSource.DocumentsById[courseId] = _courseSource.PublishedBatch[0];

        var result = await CreateSut().ReindexAsync(
            new SearchReindexRequest(null, 10));

        Assert.Equal(2, result.Scanned);
        Assert.Equal(2, result.Created);
        Assert.Contains(_repository.Documents, d => d.SourceType == SearchSourceTypes.Content);
        Assert.Contains(_repository.Documents, d => d.SourceType == SearchSourceTypes.Course);
    }

    [Fact]
    public async Task Invalid_source_type_is_rejected_before_lock()
    {
        var ex = await Assert.ThrowsAsync<SearchReindexException>(() =>
            CreateSut().ReindexAsync(new SearchReindexRequest("blog", 10)));

        Assert.Equal(SearchReindexErrorCodes.SourceInvalid, ex.Code);
        Assert.Equal(0, _lock.AcquireCallCount);
    }

    [Theory]
    [InlineData(9)]
    [InlineData(501)]
    public async Task Invalid_batch_size_is_rejected(int batchSize)
    {
        var ex = await Assert.ThrowsAsync<SearchReindexException>(() =>
            CreateSut().ReindexAsync(new SearchReindexRequest(null, batchSize)));

        Assert.Equal(SearchReindexErrorCodes.BatchSizeInvalid, ex.Code);
        Assert.Equal(0, _lock.AcquireCallCount);
    }

    [Fact]
    public async Task Batches_are_requested_deterministically_and_paged()
    {
        var ids = Enumerable.Range(1, 25)
            .Select(i => Guid.Parse($"00000000-0000-0000-0000-{i:D12}"))
            .ToList();
        _contentSource.PublishedBatch = ids
            .Select(id => Source(SearchSourceTypes.Content, id, $"T{id:N}", $"s-{id:N}"))
            .ToList();
        foreach (var item in _contentSource.PublishedBatch)
        {
            _contentSource.DocumentsById[item.SourceId] = item;
        }

        await CreateSut().ReindexAsync(new SearchReindexRequest(SearchSourceTypes.Content, 10));

        Assert.Equal(3, _contentSource.BatchCalls.Count);
        Assert.Null(_contentSource.BatchCalls[0].After);
        Assert.Equal(10, _contentSource.BatchCalls[0].Take);
        Assert.Equal(ids[9], _contentSource.BatchCalls[1].After);
        Assert.Equal(ids[19], _contentSource.BatchCalls[2].After);
        Assert.Equal(3, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Repeat_execution_is_idempotent_and_preserves_LastEventId()
    {
        var id = Guid.Parse("00000000-0000-0000-0000-000000000030");
        var source = Source(SearchSourceTypes.Content, id, "Title", "title");
        _contentSource.PublishedBatch = [source];
        _contentSource.DocumentsById[id] = source;
        var eventId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        _repository.Seed(new SearchDocument
        {
            Id = Guid.NewGuid(),
            SourceType = SearchSourceTypes.Content,
            SourceId = id,
            Title = "Title",
            Slug = "title",
            Summary = "Summary",
            Url = "/content/title",
            IsPublished = true,
            SourcePublishedAtUtc = source.PublishedAtUtc,
            SourceUpdatedAtUtc = source.UpdatedAtUtc,
            IndexedAtUtc = _clock.UtcNow,
            LastEventId = eventId,
        });

        var result = await CreateSut().ReindexAsync(
            new SearchReindexRequest(SearchSourceTypes.Content, 10));

        Assert.Equal(1, result.Scanned);
        Assert.Equal(0, result.Created);
        Assert.Equal(0, result.Updated);
        Assert.True(result.Skipped >= 1);
        Assert.Equal(eventId, Assert.Single(_repository.Documents).LastEventId);
    }

    [Fact]
    public async Task Newer_SearchDocument_is_not_overwritten()
    {
        var id = Guid.Parse("00000000-0000-0000-0000-000000000031");
        var olderSource = Source(SearchSourceTypes.Content, id, "Old", "old");
        _contentSource.PublishedBatch = [olderSource];
        _contentSource.DocumentsById[id] = olderSource;
        var newerAt = olderSource.UpdatedAtUtc.AddDays(2);
        _repository.Seed(new SearchDocument
        {
            Id = Guid.NewGuid(),
            SourceType = SearchSourceTypes.Content,
            SourceId = id,
            Title = "From Outbox",
            Slug = "from-outbox",
            Summary = "Newer",
            Url = "/content/from-outbox",
            IsPublished = true,
            SourcePublishedAtUtc = olderSource.PublishedAtUtc,
            SourceUpdatedAtUtc = newerAt,
            IndexedAtUtc = newerAt,
            LastEventId = Guid.NewGuid(),
        });

        var result = await CreateSut().ReindexAsync(
            new SearchReindexRequest(SearchSourceTypes.Content, 10));

        Assert.Equal(0, result.Updated);
        Assert.Equal("From Outbox", Assert.Single(_repository.Documents).Title);
    }

    [Fact]
    public async Task Missing_content_source_removes_Content_SearchDocument_only()
    {
        var contentId = Guid.Parse("00000000-0000-0000-0000-000000000040");
        var courseId = Guid.Parse("00000000-0000-0000-0000-000000000041");
        SeedDocument(SearchSourceTypes.Content, contentId, "c");
        SeedDocument(SearchSourceTypes.Course, courseId, "course");

        var result = await CreateSut().ReindexAsync(
            new SearchReindexRequest(SearchSourceTypes.Content, 10));

        Assert.Equal(1, result.Removed);
        Assert.DoesNotContain(_repository.Documents, d => d.SourceType == SearchSourceTypes.Content);
        Assert.Contains(_repository.Documents, d => d.SourceType == SearchSourceTypes.Course);
    }

    [Fact]
    public async Task Missing_course_source_removes_Course_SearchDocument_only()
    {
        var contentId = Guid.Parse("00000000-0000-0000-0000-000000000042");
        var courseId = Guid.Parse("00000000-0000-0000-0000-000000000043");
        SeedDocument(SearchSourceTypes.Content, contentId, "c");
        SeedDocument(SearchSourceTypes.Course, courseId, "course");
        _contentSource.PublishedBatch = [Source(SearchSourceTypes.Content, contentId, "C", "c")];
        _contentSource.DocumentsById[contentId] = _contentSource.PublishedBatch[0];

        var result = await CreateSut().ReindexAsync(
            new SearchReindexRequest(SearchSourceTypes.Course, 10));

        Assert.Equal(1, result.Removed);
        Assert.Contains(_repository.Documents, d => d.SourceType == SearchSourceTypes.Content);
        Assert.DoesNotContain(_repository.Documents, d => d.SourceType == SearchSourceTypes.Course);
    }

    [Fact]
    public async Task Unavailable_lock_returns_already_running()
    {
        _lock.AcquireSucceeded = false;

        var ex = await Assert.ThrowsAsync<SearchReindexException>(() =>
            CreateSut().ReindexAsync(new SearchReindexRequest(null, 10)));

        Assert.Equal(SearchReindexErrorCodes.AlreadyRunning, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Lock_is_released_after_exception()
    {
        _contentSource.ExceptionToThrow = new InvalidOperationException("boom");
        _repository.Seed(new SearchDocument
        {
            Id = Guid.NewGuid(),
            SourceType = SearchSourceTypes.Content,
            SourceId = Guid.Parse("00000000-0000-0000-0000-000000000050"),
            Title = "X",
            Slug = "x",
            Summary = "s",
            Url = "/content/x",
            IsPublished = true,
            SourceUpdatedAtUtc = _clock.UtcNow,
            IndexedAtUtc = _clock.UtcNow,
            LastEventId = Guid.NewGuid(),
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateSut().ReindexAsync(new SearchReindexRequest(SearchSourceTypes.Content, 10)));

        Assert.False(_lock.IsHeld);
        Assert.Equal(1, _lock.ReleaseCount);
    }

    [Fact]
    public async Task CancellationToken_is_forwarded_to_sources_and_lock()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        _contentSource.PublishedBatch =
        [
            Source(
                SearchSourceTypes.Content,
                Guid.Parse("00000000-0000-0000-0000-000000000060"),
                "T",
                "t"),
        ];
        _contentSource.DocumentsById[_contentSource.PublishedBatch[0].SourceId] =
            _contentSource.PublishedBatch[0];

        await CreateSut().ReindexAsync(
            new SearchReindexRequest(SearchSourceTypes.Content, 10),
            token);

        Assert.Equal(token, _lock.LastCancellationToken);
        Assert.Equal(token, _contentSource.LastCancellationToken);
    }

    [Fact]
    public async Task Concurrent_calls_do_not_both_run()
    {
        await using var lease = await _lock.TryAcquireAsync();
        Assert.NotNull(lease);

        var ex = await Assert.ThrowsAsync<SearchReindexException>(() =>
            CreateSut().ReindexAsync(new SearchReindexRequest(SearchSourceTypes.Content, 10)));

        Assert.Equal(SearchReindexErrorCodes.AlreadyRunning, ex.Code);
    }

    private void SeedDocument(string sourceType, Guid sourceId, string slug)
    {
        _repository.Seed(new SearchDocument
        {
            Id = Guid.NewGuid(),
            SourceType = sourceType,
            SourceId = sourceId,
            Title = slug,
            Slug = slug,
            Summary = "s",
            Url = sourceType == SearchSourceTypes.Content ? $"/content/{slug}" : $"/courses/{slug}",
            IsPublished = true,
            SourceUpdatedAtUtc = _clock.UtcNow,
            IndexedAtUtc = _clock.UtcNow,
            LastEventId = Guid.NewGuid(),
        });
    }

    private static SearchSourceDocument Source(
        string sourceType,
        Guid id,
        string title,
        string slug)
    {
        var at = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return new SearchSourceDocument(
            id,
            sourceType,
            title,
            slug,
            "Summary",
            sourceType == SearchSourceTypes.Content ? $"/content/{slug}" : $"/courses/{slug}",
            true,
            at,
            at);
    }
}
