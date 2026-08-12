using HelpDev.Modules.Content.Application.Articles;
using HelpDev.Modules.Content.Application.Articles.Dtos;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.News;
using HelpDev.Modules.Content.Application.News.Dtos;

namespace HelpDev.API.Tests.Fakes;

internal sealed class FakeArticleMetadataService : IArticleMetadataService
{
    public ArticleMetadataDto? MetadataToReturn { get; set; }

    public ContentManagementActor? LastActor { get; private set; }

    public Guid? LastContentId { get; private set; }

    public UpdateArticleMetadataRequest? LastRequest { get; private set; }

    public string? LastOperation { get; private set; }

    public Task<ArticleMetadataDto?> GetByContentIdAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastContentId = contentId;
        LastOperation = nameof(GetByContentIdAsync);
        return Task.FromResult(MetadataToReturn);
    }

    public Task<ArticleMetadataDto> CreateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateArticleMetadataRequest request,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastContentId = contentId;
        LastRequest = request;
        LastOperation = nameof(CreateAsync);
        MetadataToReturn = new ArticleMetadataDto(
            Guid.NewGuid(),
            contentId,
            request.CategoryId,
            request.DifficultyLevel,
            request.ReadingTimeMinutes,
            request.IsFeatured,
            request.AllowComments,
            request.TableOfContentsEnabled,
            DateTime.UtcNow,
            DateTime.UtcNow);
        return Task.FromResult(MetadataToReturn);
    }

    public Task<ArticleMetadataDto> UpdateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateArticleMetadataRequest request,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastContentId = contentId;
        LastRequest = request;
        LastOperation = nameof(UpdateAsync);
        MetadataToReturn ??= new ArticleMetadataDto(
            Guid.NewGuid(),
            contentId,
            request.CategoryId,
            request.DifficultyLevel,
            request.ReadingTimeMinutes,
            request.IsFeatured,
            request.AllowComments,
            request.TableOfContentsEnabled,
            DateTime.UtcNow,
            DateTime.UtcNow);
        MetadataToReturn = MetadataToReturn with
        {
            CategoryId = request.CategoryId,
            DifficultyLevel = request.DifficultyLevel,
            ReadingTimeMinutes = request.ReadingTimeMinutes,
            IsFeatured = request.IsFeatured,
            AllowComments = request.AllowComments,
            TableOfContentsEnabled = request.TableOfContentsEnabled,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        return Task.FromResult(MetadataToReturn);
    }
}

internal sealed class FakeNewsMetadataService : INewsMetadataService
{
    public NewsMetadataDto? MetadataToReturn { get; set; }

    public ContentManagementActor? LastActor { get; private set; }

    public Guid? LastContentId { get; private set; }

    public UpdateNewsMetadataRequest? LastRequest { get; private set; }

    public string? LastOperation { get; private set; }

    public Task<NewsMetadataDto?> GetByContentIdAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastContentId = contentId;
        LastOperation = nameof(GetByContentIdAsync);
        return Task.FromResult(MetadataToReturn);
    }

    public Task<NewsMetadataDto> CreateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateNewsMetadataRequest request,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastContentId = contentId;
        LastRequest = request;
        LastOperation = nameof(CreateAsync);
        MetadataToReturn = new NewsMetadataDto(
            Guid.NewGuid(),
            contentId,
            request.SourceName,
            request.SourceUrl,
            request.NewsDateUtc,
            request.Priority,
            request.ExternalReference,
            DateTime.UtcNow,
            DateTime.UtcNow);
        return Task.FromResult(MetadataToReturn);
    }

    public Task<NewsMetadataDto> UpdateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateNewsMetadataRequest request,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastContentId = contentId;
        LastRequest = request;
        LastOperation = nameof(UpdateAsync);
        MetadataToReturn ??= new NewsMetadataDto(
            Guid.NewGuid(),
            contentId,
            request.SourceName,
            request.SourceUrl,
            request.NewsDateUtc,
            request.Priority,
            request.ExternalReference,
            DateTime.UtcNow,
            DateTime.UtcNow);
        MetadataToReturn = MetadataToReturn with
        {
            SourceName = request.SourceName,
            SourceUrl = request.SourceUrl,
            NewsDateUtc = request.NewsDateUtc,
            Priority = request.Priority,
            ExternalReference = request.ExternalReference,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        return Task.FromResult(MetadataToReturn);
    }
}
