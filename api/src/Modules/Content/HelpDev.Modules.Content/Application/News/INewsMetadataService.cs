using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.News.Dtos;

namespace HelpDev.Modules.Content.Application.News;

public interface INewsMetadataService
{
    Task<NewsMetadataDto?> GetByContentIdAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default);

    Task<NewsMetadataDto> CreateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateNewsMetadataRequest request,
        CancellationToken cancellationToken = default);

    Task<NewsMetadataDto> UpdateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateNewsMetadataRequest request,
        CancellationToken cancellationToken = default);
}
