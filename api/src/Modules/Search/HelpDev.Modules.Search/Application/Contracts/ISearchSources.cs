namespace HelpDev.Modules.Search.Application.Contracts;

public interface IContentSearchSource
{
    Task<SearchSourceDocument?> GetByIdAsync(Guid contentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SearchSourceDocument>> GetPublishedBatchAsync(
        Guid? afterSourceId,
        int take,
        CancellationToken cancellationToken = default);
}

public interface ICourseSearchSource
{
    Task<SearchSourceDocument?> GetByIdAsync(Guid courseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SearchSourceDocument>> GetPublishedBatchAsync(
        Guid? afterSourceId,
        int take,
        CancellationToken cancellationToken = default);
}

public interface ILessonSearchSource
{
    Task<SearchSourceDocument?> GetByIdAsync(Guid lessonId, CancellationToken cancellationToken = default);

    /// <summary>Lesson ids belonging to a published course (for fan-out indexing).</summary>
    Task<IReadOnlyList<Guid>> ListIdsByCourseAsync(
        Guid courseId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SearchSourceDocument>> GetPublishedBatchAsync(
        Guid? afterSourceId,
        int take,
        CancellationToken cancellationToken = default);
}

public interface IToolSearchSource
{
    Task<SearchSourceDocument?> GetByIdAsync(Guid toolId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SearchSourceDocument>> GetPublishedBatchAsync(
        Guid? afterSourceId,
        int take,
        CancellationToken cancellationToken = default);
}

public interface IPromptSearchSource
{
    Task<SearchSourceDocument?> GetByIdAsync(Guid promptId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SearchSourceDocument>> GetPublishedBatchAsync(
        Guid? afterSourceId,
        int take,
        CancellationToken cancellationToken = default);
}
