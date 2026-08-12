using HelpDev.Modules.Search.Application.Contracts;
using HelpDev.Modules.Search.Application.Dtos;

namespace HelpDev.Modules.Search.Application.Queries;

public interface ISearchQueries
{
    Task<SearchResultDto> SearchAsync(
        string query,
        string? sourceType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
