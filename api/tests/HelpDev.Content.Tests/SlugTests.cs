using HelpDev.Modules.Content.Domain.ValueObjects;

namespace HelpDev.Content.Tests;

public sealed class SlugTests
{
    [Fact]
    public void Create_accepts_valid_slug()
    {
        var slug = Slug.Create("react-19-compiler");

        Assert.Equal("react-19-compiler", slug.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryCreate_rejects_null_or_whitespace(string? input)
    {
        var created = Slug.TryCreate(input, out var slug);

        Assert.False(created);
        Assert.Null(slug);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("-leading")]
    [InlineData("Trailing-")]
    [InlineData("Upper Case")]
    [InlineData("has_underscore")]
    public void TryCreate_rejects_invalid_patterns(string input)
    {
        Assert.False(Slug.TryCreate(input, out _));
    }

    [Theory]
    [InlineData("  Hello-World  ", "hello-world")]
    [InlineData("React-19", "react-19")]
    [InlineData("dotnet", "dotnet")]
    public void TryCreate_normalizes_trim_and_case(string input, string expected)
    {
        var created = Slug.TryCreate(input, out var slug);

        Assert.True(created);
        Assert.NotNull(slug);
        Assert.Equal(expected, slug!.Value);
    }

    [Fact]
    public void Create_throws_for_invalid_slug()
    {
        Assert.Throws<ArgumentException>(() => Slug.Create(" "));
    }
}
