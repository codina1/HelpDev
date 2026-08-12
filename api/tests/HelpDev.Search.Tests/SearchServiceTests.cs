using HelpDev.Modules.Search.Application.Dtos;
using HelpDev.Modules.Search.Application.Queries;
using HelpDev.Modules.Search.Application.Search;
using HelpDev.Modules.Search.Domain;

namespace HelpDev.Search.Tests;

public sealed class SearchServiceTests
{
    [Fact]
    public async Task Blank_query_is_rejected()
    {
        var service = new SearchService(new CapturingSearchQueries());

        var ex = await Assert.ThrowsAsync<SearchException>(() =>
            service.SearchAsync("   ", null, 1, 20));

        Assert.Equal(SearchErrorCodes.QueryRequired, ex.Code);
    }

    [Fact]
    public async Task Query_longer_than_200_is_rejected()
    {
        var service = new SearchService(new CapturingSearchQueries());

        var ex = await Assert.ThrowsAsync<SearchException>(() =>
            service.SearchAsync(new string('a', 201), null, 1, 20));

        Assert.Equal(SearchErrorCodes.QueryTooLong, ex.Code);
    }

    [Fact]
    public async Task Invalid_type_is_rejected()
    {
        var service = new SearchService(new CapturingSearchQueries());

        var ex = await Assert.ThrowsAsync<SearchException>(() =>
            service.SearchAsync("hello", "blog", 1, 20));

        Assert.Equal(SearchErrorCodes.TypeInvalid, ex.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Invalid_page_is_rejected(int page)
    {
        var service = new SearchService(new CapturingSearchQueries());

        var ex = await Assert.ThrowsAsync<SearchException>(() =>
            service.SearchAsync("hello", null, page, 20));

        Assert.Equal(SearchErrorCodes.PageInvalid, ex.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public async Task Invalid_page_size_is_rejected(int pageSize)
    {
        var service = new SearchService(new CapturingSearchQueries());

        var ex = await Assert.ThrowsAsync<SearchException>(() =>
            service.SearchAsync("hello", null, 1, pageSize));

        Assert.Equal(SearchErrorCodes.PageSizeInvalid, ex.Code);
    }

    [Fact]
    public async Task Valid_request_trims_query_and_normalizes_type()
    {
        var queries = new CapturingSearchQueries();
        var service = new SearchService(queries);

        await service.SearchAsync("  hello  ", "CONTENT", 2, 10);

        Assert.Equal("hello", queries.LastQuery);
        Assert.Equal(SearchSourceTypes.Content, queries.LastSourceType);
        Assert.Equal(2, queries.LastPage);
        Assert.Equal(10, queries.LastPageSize);
    }

    private sealed class CapturingSearchQueries : ISearchQueries
    {
        public string? LastQuery { get; private set; }

        public string? LastSourceType { get; private set; }

        public int LastPage { get; private set; }

        public int LastPageSize { get; private set; }

        public Task<SearchResultDto> SearchAsync(
            string query,
            string? sourceType,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            LastSourceType = sourceType;
            LastPage = page;
            LastPageSize = pageSize;
            return Task.FromResult(new SearchResultDto(query, page, pageSize, 0, []));
        }
    }
}
