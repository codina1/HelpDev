using HelpDev.API.OpenApi;

namespace HelpDev.API.Tests.OpenApi;

public sealed class SchemaIdSelectorTests
{
    [Fact]
    public void Generic_types_use_PagedResultOfX_naming()
    {
        Assert.Equal("PagedResultOfString", SchemaIdSelector.GetSchemaId(typeof(PagedResult<string>)));
        Assert.Equal("PagedResultOfInt32", SchemaIdSelector.GetSchemaId(typeof(PagedResult<int>)));
    }

    [Fact]
    public void Nested_generics_join_argument_schema_ids_with_And()
    {
        Assert.Equal(
            "PagedResultOfListOfString",
            SchemaIdSelector.GetSchemaId(typeof(PagedResult<List<string>>)));
    }

    [Fact]
    public void Arrays_use_ArrayOf_prefix()
    {
        Assert.Equal("ArrayOfString", SchemaIdSelector.GetSchemaId(typeof(string[])));
    }

    private sealed class PagedResult<T>
    {
    }
}
